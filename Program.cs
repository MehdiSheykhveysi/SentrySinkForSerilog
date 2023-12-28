using Microsoft.Extensions.Configuration;
//using Sentry;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");


//builder.WebHost.UseSentry(o =>
//{
//    o.Dsn = "Put Your DSN";
//    o.Debug = true;
//    o.TracesSampleRate = 1.0;
//});

builder.Host.UseSerilog();

Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .WriteTo.Sentry(o =>
            {
                // Debug and higher are stored as breadcrumbs (default is Information)
                o.MinimumBreadcrumbLevel = LogEventLevel.Debug;
                // Warning and higher is sent as event (default is Error)
                o.MinimumEventLevel = LogEventLevel.Warning;
                o.Dsn = "Put Your DSN";
                o.Debug = true;
            })
            .CreateLogger();

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

Log.Logger.Error("Log from Serilog 222222");

app.UseHttpsRedirection();

app.UseSerilogRequestLogging(options =>
    {
        options.GetLevel = (ctx, elapsed, ex) =>
        {
            if (ex != null || ctx.Response.StatusCode > 499)
                return LogEventLevel.Error;
            if (TimeSpan.FromMilliseconds(elapsed) > TimeSpan.FromSeconds(3))
                return LogEventLevel.Warning;
            return LogEventLevel.Information;
        };
    });

app.UseAuthorization();

app.MapControllers();
app.UseRouting();
//app.UseSentryTracing();
//SentrySdk.CaptureMessage("Hello Sentry");

app.Run();
