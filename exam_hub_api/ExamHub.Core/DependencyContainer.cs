using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TVT.Core.Db.PostgreSql.Infrastructures;
using TVT.Core.Extensions;
using TVT.Core.Filters;
using TVT.Core.Identity.PostgreSql;
using TVT.Core.Middleware;
using TVT.Core.MinioStorage;
using TVT.Core.Models;
using TVT.Core.RabbitMQ;

namespace ExamHub.Core;

/// <summary>
/// Lớp khởi động cho ứng dụng, cung cấp các phương thức để cấu hình và đăng ký các dịch vụ.
/// </summary>
public static class DependencyContainer
{
    /// <summary>
    /// Áp dụng các dịch vụ cho ứng dụng web.
    /// </summary>
    /// <param name="app">Đối tượng WebApplication để cấu hình.</param>
    /// <returns>Đối tượng WebApplication đã được cấu hình với các tùy chỉnh.</returns>
    public static void UseServices(this WebApplication app)
    {
        app.UseCustomMiddleware();
    }

    /// <param name="services">Bộ sưu tập dịch vụ để đăng ký.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Đăng ký các dịch vụ API vào `IServiceCollection`.
        /// </summary>
        /// <param name="config">Cấu hình ứng dụng.</param>
        /// <param name="isDev"></param>
        public void AddServicesApi(IConfiguration config, bool isDev = true)
        {
            services
                .AddCustomGlobalFilterControllers();
            // .AddNewtonsoftJson(opt =>
            // {
            //     opt.SerializerSettings.ContractResolver = null;
            //     opt.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Unspecified;
            // });
            services
                .AddIdentityPostgreSqlUserApi()
                .AddMinioService(config)
                .AddProjectAuthService(config)
                .AddProjectService()
                .AddMiddlewareServices()
                .AddRabbitMQService(isDev)
                ;
        }

        private IServiceCollection AddProjectService()
        {
            return services
                ;
        }

        /// <summary>
        /// Đăng ký dịch vụ xác thực JWT vào `IServiceCollection`.
        /// </summary>
        /// <param name="config">Cấu hình ứng dụng.</param>
        /// <returns>Bộ sưu tập dịch vụ đã được cấu hình.</returns>
        private IServiceCollection AddProjectAuthService(IConfiguration config)
        {
            var configAudience = config.GetSection("AudienceConfig:Audience").Get<ConfigAudience>();
            services
                .AddOptionsByName<ConfigAudience>(AppConst.AudienceKey.Audience, "AudienceConfig:Audience")
                .AddOptionsByName<ConfigAudience>(AppConst.AudienceKey.AudienceRefresh,
                    "AudienceConfig:AudienceRefresh");
            ArgumentNullException.ThrowIfNull(configAudience);
            services.AddAuthJwt(configAudience);
            return services;
        }
    }
}