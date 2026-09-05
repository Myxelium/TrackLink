using api.Data;
using Microsoft.EntityFrameworkCore;

namespace api;

public class MigrateDatabaseStartupFilter(IConfiguration configuration) : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            var logger = app.ApplicationServices.GetRequiredService<ILogger<MigrateDatabaseStartupFilter>>();
            try
            {
                var shouldMigrate = configuration.GetValue<bool>("Migrate");

                using var scope = app.ApplicationServices.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                if (shouldMigrate)
                {
                    logger.LogInformation("Applying database migrations");
                    context.Database.Migrate();
                    logger.LogInformation("Database migrations applied");
                }

                next(app);
            }
            catch (Exception exception)
            {
                logger.LogCritical(exception, "Database migration failed");
                throw;
            }
        };
    }
}