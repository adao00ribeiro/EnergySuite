using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Entities;
using EtrmService.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Application.CceeIntegration.Commands;

public class ProcessCliqCceeCsvCommand : IRequest<int>
{
    public Stream CsvStream { get; set; } = null!;
}

public class ProcessCliqCceeCsvCommandHandler : IRequestHandler<ProcessCliqCceeCsvCommand, int>
{
    private readonly IEtrmDbContext _context;

    public ProcessCliqCceeCsvCommandHandler(IEtrmDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(ProcessCliqCceeCsvCommand request, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(request.CsvStream);
        var isHeader = true;
        var importedCount = 0;

        var operations = await _context.Operations
            .Include(o => o.Counterparty)
            .ToListAsync(cancellationToken);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (isHeader)
            {
                isHeader = false;
                continue;
            }

            var values = line.Split(';');
            if (values.Length < 4) continue;

            // Mock format: Period; CounterpartyCode; OperationTicketId; CceeVolume
            // Example: 2026-08-01; AGENTE_A; a1b2c3d4-e5f6; 100.50
            if (!DateTime.TryParse(values[0], out var period)) continue;
            var counterpartyCode = values[1];
            var ticketIdString = values[2];
            if (!decimal.TryParse(values[3], out var cceeVolume)) continue;

            Guid? operationId = null;
            decimal backopsVolume = 0;

            if (Guid.TryParse(ticketIdString, out var ticketId))
            {
                var matchingOp = operations.FirstOrDefault(o => o.TicketId == ticketId);
                if (matchingOp != null)
                {
                    operationId = matchingOp.Id;
                    backopsVolume = matchingOp.VolumeMwMed;
                }
            }

            var difference = Math.Abs(backopsVolume - cceeVolume);
            var status = difference == 0 ? CceeComparisonStatus.Ok : CceeComparisonStatus.Pendente;

            var comparison = new CceeComparison
            {
                OperationId = operationId,
                CounterpartyCceeCode = counterpartyCode,
                Period = period,
                BackOpsVolume = backopsVolume,
                CceeVolume = cceeVolume,
                Difference = difference,
                Status = status
            };

            _context.CceeComparisons.Add(comparison);
            importedCount++;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return importedCount;
    }
}
