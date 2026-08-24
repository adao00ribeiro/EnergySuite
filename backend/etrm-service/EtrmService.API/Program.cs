using EtrmService.Infrastructure.Data;
using EtrmService.Application.Commands;
using Microsoft.EntityFrameworkCore;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Entity Framework (PostgreSQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Port=5432;Database=etrm_db;Username=root;Password=rootpassword";

builder.Services.AddDbContext<EtrmDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<EtrmService.Domain.Interfaces.IContractRepository, EtrmService.Infrastructure.Repositories.ContractRepository>();

// Configuração do MediatR para o CQRS
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateContractCommand).Assembly));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
