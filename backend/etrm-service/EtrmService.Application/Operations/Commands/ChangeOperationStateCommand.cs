using System;
using MediatR;
using EtrmService.Domain.Enums;

namespace EtrmService.Application.Operations.Commands;

public class ChangeOperationStateCommand : IRequest<bool>
{
    public Guid OperationId { get; set; }
    public OperationState NewState { get; set; }
}
