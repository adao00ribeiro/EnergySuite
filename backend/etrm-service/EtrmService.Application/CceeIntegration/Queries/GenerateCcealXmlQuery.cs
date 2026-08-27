using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using EtrmService.Application.CceeIntegration.DTOs;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Application.CceeIntegration.Queries;

public class GenerateCcealXmlQuery : IRequest<string>
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

public class GenerateCcealXmlQueryHandler : IRequestHandler<GenerateCcealXmlQuery, string>
{
    private readonly IEtrmDbContext _context;

    public GenerateCcealXmlQueryHandler(IEtrmDbContext context)
    {
        _context = context;
    }

    public async Task<string> Handle(GenerateCcealXmlQuery request, CancellationToken cancellationToken)
    {
        // Fetch operations that overlap with the period and are Official or Published
        var operations = await _context.Operations
            .Include(o => o.Counterparty)
            .Include(o => o.Portfolio)
            .Where(o => o.State == OperationState.Official || o.State == OperationState.Published)
            .Where(o => o.StartDate <= request.PeriodEnd && o.EndDate >= request.PeriodStart)
            .ToListAsync(cancellationToken);

        var dto = new CcealXmlDto
        {
            Contratos = operations.Select(o => new CcealContratoDto
            {
                CodigoContrato = o.TicketId.ToString(),
                AgenteComprador = o.Type == OperationType.Purchase ? "BACKOPS_COMPANY" : o.Counterparty.Cnpj,
                AgenteVendedor = o.Type == OperationType.Sale ? "BACKOPS_COMPANY" : o.Counterparty.Cnpj,
                InicioSuprimento = o.StartDate.ToString("yyyy-MM-dd"),
                FimSuprimento = o.EndDate.ToString("yyyy-MM-dd"),
                MontanteMwmed = o.VolumeMwMed
            }).ToList()
        };

        var serializer = new XmlSerializer(typeof(CcealXmlDto));
        var xmlNamespaces = new XmlSerializerNamespaces();
        xmlNamespaces.Add("", ""); // Remove namespaces

        using var stringWriter = new StringWriter();
        serializer.Serialize(stringWriter, dto, xmlNamespaces);
        
        return stringWriter.ToString();
    }
}
