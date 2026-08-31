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
using Quartz;
using Microsoft.Extensions.Http;
using Polly;

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
        services.AddScoped<IDocumentStorageService, EtrmService.Infrastructure.Storage.MinioDocumentStorageService>();

        // Domain
        services.AddScoped<IEtrmDbContext>(provider => provider.GetRequiredService<EtrmDbContext>());

        // Autenticação e Multi-Tenancy
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, EtrmService.API.Services.CurrentUserService>();

        // Keycloak Admin API
        services.AddScoped<EtrmService.API.Services.IKeycloakAdminService, EtrmService.API.Services.KeycloakAdminService>();

        // Domain Services
        services.AddScoped<EtrmService.Application.Prospect.Services.IWebhookService, WebhookService>();

        // Webhooks (BK-12b/BK-14): HttpClient nomeado sem default público.
        // BaseAddress só é definido se 'Webhooks:DefaultBaseAddress' estiver configurado;
        // caso contrário, os consumos são pulados com log (jamais um default público).
        var webhookBaseAddress = configuration["Webhooks:DefaultBaseAddress"];
        services.AddHttpClient("WebhookClient", client =>
        {
            if (!string.IsNullOrWhiteSpace(webhookBaseAddress))
                client.BaseAddress = new Uri(webhookBaseAddress);
        })
        .AddPolicyHandler((serviceProvider, _) =>
        {
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>()
                .CreateLogger("EtrmService.WebhookClient");

            return Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r =>
                    (int)r.StatusCode >= 500 || r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(
                    3,
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)), // 1s, 2s, 4s
                    (outcome, delay, attempt, _) =>
                        logger.LogWarning(
                            "Webhook POST failed (attempt {Attempt}) with status {StatusCode}; retrying in {Delay:g}.",
                            attempt, outcome.Result?.StatusCode, delay));
        });

        // Menza / Imeris Services
        services.AddScoped<EtrmService.Application.ImerisIntegration.IImerisCreditClient, EtrmService.Application.ImerisIntegration.ImerisCreditClient>();
        services.AddScoped<EtrmService.Application.Operations.Services.IWebhookNotifierService, EtrmService.Application.Operations.Services.WebhookNotifierService>();
        services.AddScoped<EtrmService.Application.Services.ITradingCopilotService, EtrmService.Application.Services.TradingCopilotService>();
        services.AddScoped<EtrmService.Application.Services.IOpportunityEngineService, EtrmService.Application.Services.OpportunityEngineService>();

        // Repositórios
        services.AddScoped<IContractRepository, ContractRepository>();
        services.AddScoped<IPortfolioRepository, PortfolioRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IWebhookRepository, WebhookRepository>();
        services.AddScoped<IHydrologyRepository, HydrologyRepository>();
        services.AddScoped<IProspectRepository, ProspectRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();


        // MediatR (CQRS)
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateContractCommand).Assembly);
            cfg.AddOpenBehavior(typeof(AuditLoggingBehavior<,>));
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
                rider.AddConsumer<EtrmService.API.Consumers.ProspectModelRunnerConsumer>();
                rider.AddConsumer<EtrmService.API.Consumers.EnaCalculatedEventConsumer>();
                rider.AddConsumer<EtrmService.API.Consumers.OperationPublishedEventConsumer>();
                rider.AddProducer<ContractCreatedIntegrationEvent>("contract-events");
                rider.AddProducer<SimulationRequestedIntegrationEvent>("pluvia-events");
                rider.AddProducer<OperationPublishedIntegrationEvent>("operation-events");
                rider.AddProducer<EtrmService.Application.Prospect.Events.StudyExecutionRequestedEvent>("study-execution-requested");

                rider.UsingKafka((context, k) =>
                {
                    k.Host(configuration["Kafka:BootstrapServers"] ?? "localhost:9092");

                    k.TopicEndpoint<RiskCalculatedIntegrationEvent>("risk-events", "etrm-service-group", e =>
                    {
                        e.ConfigureConsumer<EtrmService.API.Consumers.RiskCalculatedEventConsumer>(context);
                    });

                    k.TopicEndpoint<EnaCalculatedIntegrationEvent>("ena-events", "etrm-service-group", e =>
                    {
                        e.ConfigureConsumer<EtrmService.API.Consumers.EnaCalculatedEventConsumer>(context);
                    });

                    k.TopicEndpoint<OperationPublishedIntegrationEvent>("operation-events", "etrm-service-group", e =>
                    {
                        e.ConfigureConsumer<EtrmService.API.Consumers.OperationPublishedEventConsumer>(context);
                    });

                    k.TopicEndpoint<EtrmService.Application.Prospect.Events.StudyExecutionRequestedEvent>("study-execution-requested", "etrm-service-group", e =>
                    {
                        e.ConfigureConsumer<EtrmService.API.Consumers.ProspectModelRunnerConsumer>(context);
                    });
                });
            });
        });

        // Quartz Agendamentos
        services.AddQuartz(q =>
        {
            var jobKey = new JobKey("HydrologicalSimulationJob");
            q.AddJob<EtrmService.API.Jobs.HydrologicalSimulationJob>(opts => opts.WithIdentity(jobKey));

            // Executa todos os dias as 04:00 AM
            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("HydrologicalSimulationJob-trigger")
                .WithCronSchedule("0 0 4 * * ?"));
        });
        
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
    }
}
