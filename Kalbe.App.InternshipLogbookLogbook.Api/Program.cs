using Elastic.Apm.NetCoreAll;
using Hangfire;
using Hangfire.PostgreSql;
using HealthChecks.UI.Client;
using Kalbe.App.InternshipLogbookLogbook.Api.Auth;
using Kalbe.App.InternshipLogbookLogbook.Api.Job;
using Kalbe.App.InternshipLogbookLogbook.Api.Models;
using Kalbe.App.InternshipLogbookLogbook.Api.Services;
using Kalbe.Library.Common.Logs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRequiredServices(builder.Configuration, builder.Environment);
builder.Services.AddHangfire(opt =>
{
    opt.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("PostgreSqlConnectionString"))
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings();
});
builder.Services.AddHangfireServer();
builder.Services.AddMvc();

var app = builder.Build();

app.UseAllElasticApm(builder.Configuration);


if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
    .WithExposedHeaders("*"));

app.UseMiddleware<ExceptionLogMiddleware>();
app.UseMiddleware<InternshipLogbookLogbookJwtMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/hc");
app.MapHealthChecks("/hc-ui", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.UseHangfireDashboard();
RecurringJob.AddOrUpdate<IReminderJob>(Guid.NewGuid().ToString(),
    x => x.Execute(), Cron.Minutely);

// Force database migration
app.MigrateDbContext<InternshipLogbookLogbookDataContext>();

app.Run();
