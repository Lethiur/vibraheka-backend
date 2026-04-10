using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Domain.Subscriptions.Entities;

namespace VibraHeka.Domain.Entities;

public class SubscriptionEntity : BaseAuditableEntity
{
    public string SubscriptionID { get; set; } = string.Empty;
    
    public string UserID { get; set; } = string.Empty;
    
    public DateTimeOffset StartDate { get; set; } = DateTimeOffset.UtcNow;
    
    public DateTimeOffset EndDate { get; set; } = DateTimeOffset.UtcNow;
    
    public string ExternalSubscriptionItemID { get; set; } = string.Empty;
    
    public string ExternalSubscriptionID { get; set; } = string.Empty;
    
    public string ExternalCustomerID { get; set; } = string.Empty;
    
    public string CheckoutSessionID { get; set; } = string.Empty;
    
    public string ExternalReferenceID { get; set; } = string.Empty;

    public string CheckoutSessionUrl { get; set; } = string.Empty;

    public DateTimeOffset CheckoutSessionExpiresAt { get; set; } = DateTimeOffset.UtcNow;
    
    public OrderType OrderType { get; set; } = OrderType.Subscription;
    
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    
    public SubscriptionStatus SubscriptionStatus { get; set; } = SubscriptionStatus.Created;

    public void SetExternalCustomerID(string externalCustomerID)
    {
        ExternalCustomerID = externalCustomerID;
    }

    public void SetUserID(string userID)
    {
        UserID = userID;   
    }

    public void MarkAsCancelled()
    {
        if (Status is OrderStatus.Cancelled or OrderStatus.PaymentFailed)
        {
            throw new Exception(SubscriptionErrors.SubscriptionIsCancelled);
        }
        SubscriptionStatus = SubscriptionStatus.ToBeCancelled;
    }

    public void Reactivate()
    {
        if (Status is OrderStatus.Cancelled or OrderStatus.PaymentFailed)
        {
            throw new Exception(SubscriptionErrors.SubscriptionIsCancelled);
        }

        if (Status == OrderStatus.OrderDelayed && StartDate > DateTimeOffset.UtcNow)
        {
            SubscriptionStatus = SubscriptionStatus.Trialing;
        }
        else
        {
            SubscriptionStatus = SubscriptionStatus.Active;
        }
    }
    
    public void PrepareForCheckout(SubscriptionCheckoutSessionEntity checkoutSession)
    {
        CheckoutSessionID = checkoutSession.CheckoutSessionID;
        CheckoutSessionUrl = checkoutSession.Url;
        CheckoutSessionExpiresAt = checkoutSession.SessionExpiresAt;
        Status = OrderStatus.Pending;
        SubscriptionStatus = SubscriptionStatus.Created;
        CheckoutSessionUrl = checkoutSession.Url;
        Created = DateTime.UtcNow;
        CheckoutSessionExpiresAt = checkoutSession.SessionExpiresAt;
        ExternalSubscriptionID = checkoutSession.ItemID;
        CheckoutSessionID = checkoutSession.CheckoutSessionID;
        ExternalReferenceID = checkoutSession.InternalReferenceID;
    }
    
}
