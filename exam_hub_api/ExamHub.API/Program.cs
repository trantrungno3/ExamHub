using ExamHub.Core;
using Scalar.AspNetCore;
using TVT.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(op => { op.AddAuthOpenApiDoc(); });
builder.Services.AddServicesApi(builder.Configuration, builder.Environment.IsDevelopment());
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "all",
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
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