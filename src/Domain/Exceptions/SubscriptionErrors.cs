namespace VibraHeka.Domain.Exceptions;

public class SubscriptionErrors
{
    public static readonly string NoSubscriptionFound = "S-000";
    public static readonly string ErrorWhileSubscribing = "S-001";
    public static readonly string SubscriptionIsActive = "S-002";
    public static readonly string SubscriptionIsCancelled = "S-003";
}
