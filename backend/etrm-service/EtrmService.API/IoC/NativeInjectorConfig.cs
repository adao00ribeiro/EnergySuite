using EtrmService.Application.Commands;
using EtrmService.Application.Behaviors;
using EtrmService.Application.Validators;
using EtrmService.Application.Interfaces;
using EtrmService.Infrastructure.Messaging;
using EtrmService.Infrastructure.Data;
using EtrmService.Infrastructure.Repositories;
using EtrmService.Infrastructure.Services;
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

        // Amazon S3 (MinIO)
        var s3Config = new Amazon.S3.AmazonS3Config
        {
            ServiceURL = Environment.GetEnvironmentVariable("MINIO_ENDPOINT") ?? "http://localhost:9000",
            ForcePathStyle = true
        };
        services.AddSingleton<Amazon.S3.IAmazonS3>(sp => new Amazon.S3.AmazonS3Client("minioadmin", "minioadmin", s3Config));
        services.AddScoped<IBlobStorageService, S3BlobStorageService>();

        // Domain
        services.AddScoped<IEtrmDbContext>(provider => provider.GetRequiredService<EtrmDbContext>());

        // Autenticação e Multi-Tenancy
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, EtrmService.API.Services.CurrentUserService>();

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
                rider.AddConsumer<EtrmService.API.Consumers.EnaCalculatedEventConsumer>();
                rider.AddProducer<ContractCreatedIntegrationEvent>("contract-events");
                rider.AddProducer<SimulationRequestedIntegrationEvent>("pluvia-events");

                rider.UsingKafka((context, k) =>
                {
                    k.Host(kafkaBootstrapServers);

                    k.TopicEndpoint<RiskCalculatedIntegrationEvent>("risk-events", "etrm-group", e =>
                    {
                        e.ConfigureConsumer<EtrmService.API.Consumers.RiskCalculatedEventConsumer>(context);
                    });

                    k.TopicEndpoint<EnaCalculatedIntegrationEvent>("ena-events", "etrm-group", e =>
                    {
                        e.ConfigureConsumer<EtrmService.API.Consumers.EnaCalculatedEventConsumer>(context);
                    });
                });
            });
        });
    }
}
