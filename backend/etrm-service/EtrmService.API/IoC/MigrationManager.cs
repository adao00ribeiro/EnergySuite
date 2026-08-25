using Microsoft.EntityFrameworkCore;

namespace EtrmService.API.IoC;

public static class MigrationManager
{
    public static IHost MigrateDatabase<TContext>(this IHost host) where TContext : DbContext
    {
        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<TContext>();
                context.Database.Migrate();
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<TContext>>();
                logger.LogError(ex, "Ocorreu um erro ao rodar as migrações do banco de dados.");
                throw;
            }
        }
        return host;
    }
}
