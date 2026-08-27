namespace EtrmService.Domain.Enums;

public enum DeckState
{
    Pending = 1,
    Generating = 2,
    Ready = 3,
    Running = 4,
    Completed = 5,
    Failed = 6,
    Infeasible = 7
}
