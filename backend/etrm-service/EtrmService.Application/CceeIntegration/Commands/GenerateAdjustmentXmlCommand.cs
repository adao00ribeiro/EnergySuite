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

namespace EtrmService.Application.CceeIntegration.Commands;

public class GenerateAdjustmentXmlCommand : IRequest<string>
{
    public List<Guid> ComparisonIds { get; set; } = new();
}

public class GenerateAdjustmentXmlCommandHandler : IRequestHandler<GenerateAdjustmentXmlCommand, string>
{
    private readonly IEtrmDbContext _context;

    public GenerateAdjustmentXmlCommandHandler(IEtrmDbContext context)
    {
        _context = context;
    }

    public async Task<string> Handle(GenerateAdjustmentXmlCommand request, CancellationToken cancellationToken)
    {
        var comparisons = await _context.CceeComparisons
            .Where(c => request.ComparisonIds.Contains(c.Id))
            .Where(c => c.Status == CceeComparisonStatus.Pendente)
            .ToListAsync(cancellationToken);

        if (!comparisons.Any())
            return string.Empty;

        var dto = new CcealXmlDto
        {
            Contratos = comparisons.Select(c => new CcealContratoDto
            {
                CodigoContrato = c.OperationId?.ToString() ?? "N/A",
                AgenteComprador = "BACKOPS_TENANT", // Simplified for this implementation
                AgenteVendedor = c.CounterpartyCceeCode ?? "UNKNOWN",
                InicioSuprimento = c.Period.ToString("yyyy-MM-dd"),
                FimSuprimento = c.Period.ToString("yyyy-MM-dd"),
                MontanteMwmed = c.Difference // The adjustment is the difference
            }).ToList()
        };

        foreach (var comparison in comparisons)
        {
            comparison.UpdateStatus(CceeComparisonStatus.Ajustado);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var serializer = new XmlSerializer(typeof(CcealXmlDto));
        var xmlNamespaces = new XmlSerializerNamespaces();
        xmlNamespaces.Add("", ""); // Remove namespaces

        using var stringWriter = new StringWriter();
        serializer.Serialize(stringWriter, dto, xmlNamespaces);
        
        return stringWriter.ToString();
    }
}
