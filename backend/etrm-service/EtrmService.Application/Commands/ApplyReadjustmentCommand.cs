using MediatR;
using System;

namespace EtrmService.Application.Commands;

public record ApplyReadjustmentCommand(Guid ContractId, decimal NewPrice, string Description, DateTime EffectiveDate) : IRequest<bool>;
