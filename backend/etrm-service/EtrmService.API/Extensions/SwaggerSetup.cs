using Microsoft.OpenApi.Models;

namespace EtrmService.API.Extensions;

public static class SwaggerSetup
{
    public static void AddSwaggerSetup(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "ETRM API", Version = "v1" });
        });
    }
}
