using Infrastructure.Rest.Client.Stripe.Models;
using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Orders.Models;

namespace Infrastructure.Rest.Client.Stripe.Mappers;

[Mapper]
public partial class StripeMapper
{
    [MapProperty(nameof(UserEntity.Id), nameof(RegisterCustomerRequest.UserID))]
    [MapperIgnoreSource(nameof(UserEntity.CustomerID))]
    [MapperIgnoreSource(nameof(UserEntity.ProfilePictureUrl))]
    [MapperIgnoreSource(nameof(UserEntity.Bio))]
    [MapperIgnoreSource(nameof(UserEntity.TimezoneID))]
    [MapperIgnoreSource(nameof(UserEntity.Role))]
    [MapperIgnoreSource(nameof(UserEntity.Created))]
    [MapperIgnoreSource(nameof(UserEntity.CreatedBy))]
    [MapperIgnoreSource(nameof(UserEntity.LastModified))]
    [MapperIgnoreSource(nameof(UserEntity.LastModifiedBy))]
    public partial RegisterCustomerRequest FromUserEntityToRegisterCustomerRequest(UserEntity entity);
    
    [MapProperty(nameof(model.Quantity), nameof(StartOrderRequest.OrderQuantity))]
    [MapProperty(nameof(model.ProductRef), nameof(StartOrderRequest.PriceRef))]
    public partial StartOrderRequest FromDomainToStartOrderRequest(CheckoutProductModel model, List<string> PaymentMethodsAccepted);
    
    [MapProperty(nameof(checkoutResult.Url), nameof(CheckoutSessionCompletedModel.CheckoutUrl))]
    [MapProperty(nameof(checkoutResult.InternalPaymentID), nameof(CheckoutSessionCompletedModel.PaymentIntentID))]
    [MapperIgnoreSource(nameof(CheckoutResult.ExpiresAt))]
    [MapperIgnoreSource(nameof(CheckoutResult.PaymentSessionID))]
    public partial CheckoutSessionCompletedModel FromCheckoutResultToCheckoutSessionCompletedModel(CheckoutResult checkoutResult);
}
