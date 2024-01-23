using Hangfire.Server;
using System.Linq.Expressions;
using Hangfire;
using Hangfire.PostgreSql;

using Kalbe.App.InternshipLogbookLogbook.Api.Job;


var host = Host.CreateDefaultBuilder(args);
var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

host.ConfigureServices(services =>
{
    //feetch config
    services.AddHangfire(opt =>
    {
        opt.UsePostgreSqlStorage(configuration.GetConnectionString("PostgreSqlConnectionString"))
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings();
    });
    services.AddHangfireServer();
    services.AddScoped<IReminderJob, ReminderJob>();

});


host.Build().Run();
