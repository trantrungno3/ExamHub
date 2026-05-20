using ExamHub.Core;
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
        policy.Requirements.Add(new ExamHub.API.Authorization.TeacherOwnsSubjectRequirement()));

builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler,
    ExamHub.API.Authorization.TeacherOwnsSubjectHandler>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "all",
        policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/docs");
}

app.UseServices();
app.UseHttpsRedirection();
app.UseCors("all");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();