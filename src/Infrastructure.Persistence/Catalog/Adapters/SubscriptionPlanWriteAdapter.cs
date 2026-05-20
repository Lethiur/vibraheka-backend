using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Persistence.Catalog.Mappers;
using Infrastructure.Persistence.Catalog.Models;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Application.Catalog.Ports.Out;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Infrastructure.Entities;
namespace Infrastructure.Persistence.Catalog.Adapters;

public class SubscriptionPlanWriteAdapter(
    SubscriptionPlanEntityMapper Mapper,
    AWSConfig Config,
    IDynamoDBContext Context) : ISubscriptionPlanWritePort
{
    public ITransactionalWriteOperation CreateSubscriptionPlan(SubscriptionPlanEntity subscriptionPlan)
    {
        SubscriptionPlanDBModel model = Mapper.FromDomain(subscriptionPlan);
        ITransactWrite<SubscriptionPlanDBModel> transaction =
            Context.CreateTransactWrite<SubscriptionPlanDBModel>(new TransactWriteConfig
            {
                OverrideTableName = Config.SubscriptionPlansTable
            });
        transaction.AddSaveItem(model);
        return new DynamoTransactionalWriteOperation(transaction);
    }
}
