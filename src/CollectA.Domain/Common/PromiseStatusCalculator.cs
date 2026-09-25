using CollectA.Domain.Entities;
using CollectA.Domain.Enums;

namespace CollectA.Domain.Common;

public static class PromiseStatusCalculator
{
    public static PromiseStatus Calculate(PromiseToPay promise, decimal fulfilledAmount)
    {
        if (promise.Status == PromiseStatus.Cancelled)
            return PromiseStatus.Cancelled;

        if (fulfilledAmount <= 0)
            return PromiseStatus.Broken;

        if (fulfilledAmount >= promise.PromisedAmount)
            return PromiseStatus.Fulfilled;

        return PromiseStatus.PartiallyFulfilled;
    }
}
