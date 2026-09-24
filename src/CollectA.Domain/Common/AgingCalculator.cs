namespace CollectA.Domain.Common;

public static class AgingCalculator
{
    public const string Current = "Current";
    public const string Days1To30 = "1-30 days";
    public const string Days31To60 = "31-60 days";
    public const string Days61To90 = "61-90 days";
    public const string Days91To120 = "91-120 days";
    public const string Days121To180 = "121-180 days";
    public const string Over180 = ">180 days";

    public static string GetBucket(int daysOverdue) => daysOverdue switch
    {
        <= 0 => Current,
        <= 30 => Days1To30,
        <= 60 => Days31To60,
        <= 90 => Days61To90,
        <= 120 => Days91To120,
        <= 180 => Days121To180,
        _ => Over180
    };
}
