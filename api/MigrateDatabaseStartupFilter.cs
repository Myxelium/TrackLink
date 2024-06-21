using api.Data;
using Microsoft.EntityFrameworkCore;

namespace api;

public class MigrateDatabaseStartupFilter(IConfiguration configuration) : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            var shouldMigrate = configuration.GetValue<bool>("Migrate");
            
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            
            if (shouldMigrate)
            {
                context.Database.Migrate();
            }

            // Continue with the next middleware
            next(app);
        };
    }
}