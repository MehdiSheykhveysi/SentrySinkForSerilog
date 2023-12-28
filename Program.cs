using Microsoft.Extensions.Configuration;
//using Sentry;
using Serilog;
using Serilog.Events;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");


//builder.WebHost.UseSentry(o =>
//{
//    o.Dsn = "Put Your DSN";
//    o.Debug = true;
//    o.TracesSampleRate = 1.0;
//});

builder.Host.UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    //.WriteTo.Console()
                    //.WriteTo.Sentry(s =>
                    //{
                    //    s.Dsn = "";
                    //    s.AttachStacktrace = true;
                    //    s.Debug = true;
                    //    s.EnableTracing = true;
                    //    s.SendDefaultPii = true;
                    //    s.DiagnosticLevel = Sentry.SentryLevel.Debug;
                    //})
                    );

Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine(msg));

var file = File.CreateText($"{builder.Environment.ContentRootPath}/{"InternalSerilogErrors.txt"}");
Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(file));

Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateBootstrapLogger();

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
