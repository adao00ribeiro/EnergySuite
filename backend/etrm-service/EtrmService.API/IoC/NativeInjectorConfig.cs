using EtrmService.Application.Commands;
using EtrmService.Application.Behaviors;
using EtrmService.Application.Validators;
using EtrmService.Application.Interfaces;
using EtrmService.Infrastructure.Messaging;
using EtrmService.Infrastructure.Data;
using EtrmService.Infrastructure.Repositories;
using EtrmService.Domain.Interfaces;
using EtrmService.Application.IntegrationEvents;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using FluentValidation;

namespace EtrmService.API.IoC;

public static class NativeInjectorConfig
{
    public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Banco de Dados
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Host=localhost;Port=5432;Database=etrm_db;Username=root;Password=rootpassword";
            
        services.AddDbContext<EtrmDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositórios
        services.AddScoped<IContractRepository, ContractRepository>();

        // MediatR (CQRS)
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateContractCommand).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // Validações
        services.AddValidatorsFromAssembly(typeof(CreateContractCommandValidator).Assembly);

        // Mensageria (Kafka / MassTransit)
        services.AddScoped<IEventPublisher, KafkaEventPublisher>();
        
        var kafkaBootstrapServers = configuration.GetSection("Kafka:BootstrapServers").Value ?? "localhost:9092";
        services.AddMassTransit(x =>
        {
            x.UsingInMemory((context, cfg) => 
            {
                cfg.ConfigureJsonSerializerOptions(options =>
                {
                    options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                    return options;
                });
                cfg.ConfigureEndpoints(context);
            });

            x.AddRider(rider =>
            {
                rider.AddConsumer<EtrmService.API.Consumers.RiskCalculatedEventConsumer>();
                rider.AddProducer<ContractCreatedIntegrationEvent>("contract-events");

                rider.UsingKafka((context, k) =>
                {
                    k.Host(kafkaBootstrapServers);

                    k.TopicEndpoint<RiskCalculatedIntegrationEvent>("risk-events", "etrm-group", e =>
                    {
                        e.ConfigureConsumer<EtrmService.API.Consumers.RiskCalculatedEventConsumer>(context);
                    });
                });
            });
        });
    }
}
