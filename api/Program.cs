using System.Reflection;
using api;
using api.Data;
using api.Integrations.Google;
using api.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

var logsDirectory = Path.Combine(builder.Environment.ContentRootPath, "logs");
Directory.CreateDirectory(logsDirectory);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(logsDirectory, "tracklink-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        shared: true)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(
    configuration => configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
);
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));
builder.Services.AddSingleton<IStartupFilter, MigrateDatabaseStartupFilter>();
builder.Services.AddSingleton<IStartupFilter, SeedDataStartupFilter>();
builder.Services.Configure<GoogleOptions>(builder.Configuration.GetSection(GoogleOptions.SectionName));
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.SectionName));
builder.Services.AddDataProtection();
builder.Services.AddScoped<IGoogleDriveService, GoogleDriveService>();
builder.Services.AddScoped<IAudioPlaybackService, AudioPlaybackService>();
builder.Services.AddScoped<IMemberSession, MemberSession>();
builder.Services.AddScoped<IInviteMailer, InviteMailer>();
builder.Services.AddHttpClient("audio", client =>
{
    client.Timeout = TimeSpan.FromMinutes(2);
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.Use(async (context, next) =>
{
    try
    {
        await next();
        if (context.Response.StatusCode >= 500)
        {
            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("api.Http");
            logger.LogError(
                "HTTP {StatusCode} {Method} {Path}",
                context.Response.StatusCode,
                context.Request.Method,
                context.Request.Path.Value);
        }
    }
    catch (Exception exception)
    {
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("api.Http");
        logger.LogError(
            exception,
            "Unhandled exception {Method} {Path}",
            context.Request.Method,
            context.Request.Path.Value);
        if (context.Response.HasStarted)
        {
            throw;
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error = "Something went wrong. Try again." });
    }
});

app.UseAuthorization();
app.MapControllers();
app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

try
{
    Log.Information("TrackLink API starting");
    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "TrackLink API terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
