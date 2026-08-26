using MediatR;
using System;

namespace EtrmService.Application.Finance.Commands;

public record ExecuteAccountOffsetCommand(
    Guid CounterpartyId,
    string ReferenceMonth
) : IRequest<Guid?>;
