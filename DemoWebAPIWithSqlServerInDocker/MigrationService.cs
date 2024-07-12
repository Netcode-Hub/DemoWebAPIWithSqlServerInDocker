using Microsoft.EntityFrameworkCore;

namespace DemoWebAPIWithSqlServerInDocker
{
    public class MigrationService
    {
        public static void InitializeMigration(IApplicationBuilder app)

        {
            using var serviceScope = app.ApplicationServices.CreateScope();
            serviceScope.ServiceProvider.GetService<ProductDbContext>()!.Database.Migrate();
        }
    }
}
