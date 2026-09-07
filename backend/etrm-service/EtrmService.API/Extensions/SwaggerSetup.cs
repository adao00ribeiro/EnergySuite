using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace EtrmService.API.Extensions;

public static class SwaggerSetup
{
    public static void AddSwaggerSetup(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "EnergySuite — Unified ETRM & B2B Energy API",
                Version = "v1",
                Description = "Especificação OpenAPI 3.0 unificada dos Bounded Contexts do EnergySuite:\n" +
                              "• Trading & Portfólio (Menza)\n" +
                              "• Hidrologia & ENA (Pluvia)\n" +
                              "• Riscos & MtM (Imeris)\n" +
                              "• Operações & Conciliação (CCEE BackOps)",
                Contact = new OpenApiContact
                {
                    Name = "EnergySuite Engineering Squad",
                    Email = "architecture@energysuite.com.br"
                }
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Autenticação via JWT Keycloak. Informe 'Bearer {token}' no campo abaixo.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new List<string>()
                }
            });
        });
    }
}
