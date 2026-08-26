using MediatR;
using System;

namespace EtrmService.Application.Finance.Commands;

public record GenerateBillingCommand(
    Guid OperationId,
    string ReferenceMonth,
    decimal CalculatedVolume,
    decimal AppliedPrice,
    decimal TaxesAmount
) : IRequest<Guid>;
