using DinkToPdf;
using DinkToPdf.Contracts;
using Kalbe.App.InternshipLogbookLogbook.Api.Models;
using Kalbe.App.InternshipLogbookLogbook.Api.Models.Commons;
using Kalbe.App.InternshipLogbookLogbook.Api.Utilities;
using Kalbe.Library.Common.Logs;
using Kalbe.Library.Data.EntityFrameworkCore.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;
using System.Reflection;

namespace Kalbe.App.InternshipLogbookLogbook.Api.Services
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddRequiredServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            services.AddHealthChecks()
                .AddDbContextCheck<InternshipLogbookLogbookDataContext>(name: "Database")
                .AddRedis(configuration.GetConnectionString("RedisServer"), name: "Redis Cache");

            services.AddControllers().AddNewtonsoftJson(
                options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(
                opt =>
                {
                    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

                    // TODO: Change this swagger document definition
                    opt.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                    {
                        Title = "E-Logbook Logbook API",
                        Description = "API for Internship Logbook. Logbook Module",
                        Version = "v1"
                    });
                });

            services.AddStackExchangeRedisCache(
                opt =>
                {
                    opt.Configuration = configuration.GetConnectionString("RedisServer");
                });

            services.AddHttpContextAccessor();
            services.RegisterLogger(configuration, environment);
            services.RegisterDatabase<InternshipLogbookLogbookDataContext>(configuration, environment, DatabaseProvider.PostgreSql, "PostgreSqlConnectionString");

            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();

                x.UsingRabbitMq((context, cfg) =>
                {
                    var config = configuration.GetSection("RabbitMQ").Get<RabbitMqConfiguration>();
                    cfg.Host(config.HostName, config.VirtualHost,
                        configurator =>
                        {
                            configurator.Username(config.UserName);
                            configurator.Password(config.Password);
                        });

                    cfg.ConfigureEndpoints(context);
                });
            });

            services.AddScoped<IInternshipLogbookLogbookService, InternshipLogbookLogbookService>();
            services.AddScoped<ILogbookDaysService, LogbookDaysService>();
            services.AddScoped<ILoggerHelper, LoggerHelper>();
            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            services.AddScoped<IPDFGenerator, PDFGenerator>();
            return services;
        }

        public static void MigrateDbContext<T>(this IApplicationBuilder applicationBuilder) where T : DbContext
        {
            using var scope = applicationBuilder.ApplicationServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<T>();
            dbContext.Database.Migrate();
        }
    }
}
