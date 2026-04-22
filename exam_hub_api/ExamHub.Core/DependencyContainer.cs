using ExamHub.Core.Infrastructure.Caching;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;
using ExamHub.Core.Infrastructure.Persistence.Services.Implementations;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using System.Reflection;
using TVT.Core.Extensions;
using TVT.Core.Filters;
using TVT.Core.Identity.PostgreSql;
using TVT.Core.Middleware;
using TVT.Core.MinioStorage;
using TVT.Core.Models;
using TVT.Core.RabbitMQ;

namespace ExamHub.Core;

/// <summary>
/// Điểm khởi động duy nhất — đăng ký toàn bộ services cho ExamHub.Core.
/// </summary>
public static class DependencyContainer
{
    /// <summary>Áp dụng middleware pipeline.</summary>
    public static void UseServices(this WebApplication app)
    {
        app.UseCustomMiddleware();
    }

    /// <param name="services">Bộ sưu tập dịch vụ để đăng ký.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Đăng ký toàn bộ services: cross-cutting, CQRS, EF Core, Repositories, Application Services, Storage, Cache.
        /// </summary>
        public void AddServicesApi(IConfiguration config, bool isDev = true)
        {
            services
                .AddIdentityPostgreSqlUserApi()
                .AddMinioService(config)
                .AddMiddlewareServices()
                .AddRabbitMQService(isDev)
                .AddProjectAuthService(config)
                .AddCqrsServices()
                .AddAppDbContext(config)
                .AddRedisCache(config)
                .AddRepositories()
                .AddAppServices()
                .AddCustomGlobalFilterControllers();
        }

        private IServiceCollection AddCqrsServices()
        {
            var assembly = Assembly.Load("ExamHub.Core");
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
            services.AddValidatorsFromAssembly(assembly);
            return services;
        }

        private IServiceCollection AddAppDbContext(IConfiguration config)
        {
            var cs = config.GetSection("PostgreSqlConfig:ConnectionString").Value
                ?? throw new InvalidOperationException(
                    "PostgreSQL connection string 'PostgreSqlConfig:ConnectionString' is not configured.");
            services.AddDbContext<AppDbContext>(opt =>
                opt.UseNpgsql(cs, npgsql =>
                    {
                        npgsql.EnableRetryOnFailure(maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10), errorCodesToAdd: null);
                        npgsql.CommandTimeout(60);
                    })
                    .UseSnakeCaseNamingConvention());
            return services;
        }

        private IServiceCollection AddRedisCache(IConfiguration config)
        {
            var conn = config["Redis:ConnectionString"];
            if (string.IsNullOrWhiteSpace(conn)) return services;
            services.AddStackExchangeRedisCache(opt => opt.Configuration = conn);
            services.AddSingleton<RedisCacheService>();
            return services;
        }

        private IServiceCollection AddRepositories()
        {
            return services
                .AddScoped<IGradeLevelRepository, GradeLevelRepository>()
                .AddScoped<ISubjectRepository, SubjectRepository>()
                .AddScoped<ITopicRepository, TopicRepository>()
                .AddScoped<IDifficultyLevelRepository, DifficultyLevelRepository>()
                .AddScoped<IQuestionTypeRepository, QuestionTypeRepository>()
                .AddScoped<IQuestionRepository, QuestionRepository>()
                .AddScoped<IQuestionAnswerRepository, QuestionAnswerRepository>()
                .AddScoped<ITeacherSubjectRepository, TeacherSubjectRepository>()
                .AddScoped<IExamTemplateRepository, ExamTemplateRepository>()
                .AddScoped<IExamTemplateSectionRepository, ExamTemplateSectionRepository>()
                .AddScoped<IExamRepository, ExamRepository>()
                .AddScoped<IExamQuestionRepository, ExamQuestionRepository>()
                .AddScoped<IExamSubmissionRepository, ExamSubmissionRepository>()
                .AddScoped<ISubmissionAnswerRepository, SubmissionAnswerRepository>();
        }

        private IServiceCollection AddAppServices()
        {
            return services
                .AddScoped<IGradeLevelService, GradeLevelService>()
                .AddScoped<ISubjectService, SubjectService>()
                .AddScoped<ITopicService, TopicService>()
                .AddScoped<IDifficultyLevelService, DifficultyLevelService>()
                .AddScoped<IQuestionTypeService, QuestionTypeService>()
                .AddScoped<IQuestionService, QuestionService>()
                .AddScoped<ITeacherSubjectService, TeacherSubjectService>()
                .AddScoped<IExamTemplateService, ExamTemplateService>()
                .AddScoped<IExamService, ExamService>()
                .AddScoped<IExamSubmissionService, ExamSubmissionService>();
        }

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