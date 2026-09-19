using System.Threading.RateLimiting;
using ExamHub.API.Health;
using ExamHub.Core;
using ExamHub.Core.Infrastructure.Persistence;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TVT.Core.Extensions;
using TVT.Core.Filters;
using TVT.Core.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.GetSection("AudienceConfig:Audience").Bind(AppCommon.Audience);
builder.Configuration.GetSection("AudienceConfig:AudienceRefresh").Bind(AppCommon.AudienceRefresh);
AppCommon.SaltPassHash = builder.Configuration.GetValue<string>("SaltPassHash");

builder.Services.AddCustomGlobalFilterControllers();
builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddOpenApi(op => { op.AddAuthOpenApiDoc(); });
builder.Services.AddServicesApi(builder.Configuration, builder.Environment.IsDevelopment());
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build())
    .AddPolicy("TeacherOwnsSubject", policy =>
        policy.Requirements.Add(new ExamHub.API.Authorization.TeacherOwnsSubjectRequirement()))
    .AddPolicy("TeacherOwnsCohortClass", policy =>
        policy.Requirements.Add(new ExamHub.API.Authorization.TeacherOwnsCohortClassRequirement()));

builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler,
    ExamHub.API.Authorization.TeacherOwnsSubjectHandler>();
builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler,
    ExamHub.API.Authorization.TeacherOwnsCohortClassHandler>();
// Cookie refresh chỉ được gửi kèm khi CORS cho phép credentials, và AllowCredentials KHÔNG hợp lệ
// cùng AllowAnyOrigin — nên bắt buộc phải có allowlist origin tường minh.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
if (allowedOrigins.Length == 0 && !builder.Environment.IsDevelopment())
    throw new InvalidOperationException(
        "Cors:AllowedOrigins trống. Cấu hình origin của frontend trước khi chạy ngoài Development.");

builder.Services.AddCors(options =>
{
    options.AddPolicy("web", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseReadyCheck>("database", tags: ["ready"]);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Đăng nhập chưa có danh tính nên chỉ phân vùng được theo IP. Giới hạn phải chịu được cả một
    // phòng máy của trường sau cùng một IP NAT đăng nhập đầu giờ — 10/phút sẽ chặn oan cả lớp.
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 60,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        }));

    // Phân vùng theo username trước, IP chỉ là fallback: autosave chạy ~20s/lần cho mỗi học sinh
    // (~3 request/phút), nên 60/phút/người là thừa sức, còn nếu phân vùng theo IP thì một phòng thi
    // 30 máy sau NAT sẽ vượt hạn mức giữa giờ làm bài.
    options.AddPolicy("write-heavy", context => RateLimitPartition.GetFixedWindowLimiter(
        context.User.Identity?.IsAuthenticated == true
            ? context.User.GetUserName()
            : context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 60,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        }));
});

var app = builder.Build();

// Bootstrap schema bằng migration — opt-in để production không bao giờ tự migrate khi khởi động.
if (builder.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    await using var migrationScope = app.Services.CreateAsyncScope();
    await migrationScope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/docs");
}

// Header tối thiểu cho API: chặn MIME sniffing và nhúng iframe. CSP thuộc reverse proxy của
// frontend (plan operability), không đặt ở đây.
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    await next();
});

if (!app.Environment.IsDevelopment())
    app.UseHsts();

app.UseServices();
app.UseHttpsRedirection();
app.UseCors("web");
app.UseAuthentication();
app.UseAuthorization();
// Sau UseAuthentication để policy write-heavy phân vùng được theo username.
app.UseRateLimiter();

// Liveness không gọi dependency nào — chỉ trả lời "process còn sống". Readiness mới kiểm database.
// Cả hai AllowAnonymous vì fallback policy của app là RequireAuthenticatedUser, còn orchestrator
// probe thì không có token.
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = HealthResponse.WriteAsync,
}).AllowAnonymous();

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthResponse.WriteAsync,
}).AllowAnonymous();

app.MapControllers();
app.Run();