using Infrastructure.Rest.Client.Stripe.Models;
using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Entities;

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
    
}
