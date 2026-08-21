namespace CollectA.Domain.Enums;

public enum CollectionActionOutcome
{
    None,
    NoAnswer,
    PromisedPayment,
    PaymentReceived,
    Dispute,
    WrongContact,
    Refused,
    CustomerUnreachable,
    Escalate,
    Other
}
