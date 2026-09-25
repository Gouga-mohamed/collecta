using CollectA.Domain.Enums;

namespace CollectA.Domain.Common;

public static class DisputeWorkflow
{
    private static readonly HashSet<DisputeStatus> ActiveStatuses = new()
    {
        DisputeStatus.Open,
        DisputeStatus.Investigating,
        DisputeStatus.WaitingCustomer,
        DisputeStatus.WaitingInternal
    };

    private static readonly HashSet<DisputeStatus> TerminalStatuses = new()
    {
        DisputeStatus.Resolved,
        DisputeStatus.Closed
    };

    public static bool CanTransitionTo(DisputeStatus from, DisputeStatus to)
    {
        if (from == to) return true;
        if (TerminalStatuses.Contains(from) && !TerminalStatuses.Contains(to)) return false;
        if (TerminalStatuses.Contains(to)) return true;

        return true;
    }

    public static bool IsActive(DisputeStatus status) => ActiveStatuses.Contains(status);
}
