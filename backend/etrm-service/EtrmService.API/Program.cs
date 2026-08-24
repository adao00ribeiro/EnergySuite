using EtrmService.Infrastructure.Data;
using EtrmService.Application.Commands;
using Microsoft.EntityFrameworkCore;
using MediatR;

using MassTransit;
using FluentValidation;
using EtrmService.Application.IntegrationEvents;
using EtrmService.Application.Interfaces;
using EtrmService.Application.Validators;
using EtrmService.Application.Behaviors;
using EtrmService.API.Middleware;
using EtrmService.Infrastructure.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Entity Framework (PostgreSQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Port=5432;Database=etrm_db;Username=root;Password=rootpassword";

builder.Services.AddDbContext<EtrmDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<EtrmService.Domain.Interfaces.IContractRepository, EtrmService.Infrastructure.Repositories.ContractRepository>();

// Configuração do MediatR para o CQRS com Pipeline de Validação
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(CreateContractCommand).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// Configuração do FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(CreateContractCommandValidator).Assembly);

// Configuração do MassTransit (Kafka)
builder.Services.AddScoped<IEventPublisher, KafkaEventPublisher>();

var kafkaBootstrapServers = builder.Configuration.GetSection("Kafka:BootstrapServers").Value ?? "localhost:9092";

builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));

    x.AddRider(rider =>
    {
        rider.AddProducer<ContractCreatedIntegrationEvent>("contract-events");

        rider.UsingKafka((context, k) =>
        {
            k.Host(kafkaBootstrapServers);
        });
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

// Middleware Global de Tratamento de Erros (incluindo Validações)
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
