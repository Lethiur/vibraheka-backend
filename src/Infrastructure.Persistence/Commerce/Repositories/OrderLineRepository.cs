using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Commerce.Mappers;
using Infrastructure.Persistence.Commerce.Models;
using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Commerce.Entities;
using VibraHeka.Domain.Commerce.Errors;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace Infrastructure.Persistence.Commerce.Repositories;

/// <summary>
/// Repository class for interacting with order line data stored in an AWS DynamoDB database.
/// Provides functionality for saving multiple OrderLineEntity instances to the database.
/// </summary>
public class OrderLineRepository(
    IAmazonDynamoDB client,
    IDynamoDBContext context,
    OrderLineMapper mapper,
    ILogger<OrderLineRepository> logger)
    : GenericDynamoRepository<OrderLineDBModel>(context, client, logger)
{
    /// <summary>
    /// Saves a list of order line entities to a DynamoDB table asynchronously.
    /// Maps the domain entities to their corresponding database models,
    /// saves them in bulk to the database, and returns the original entities
    /// upon successful completion or an error result if the operation fails.
    /// </summary>
    /// <param name="lines">
    /// A list of <see cref="OrderLineEntity"/> instances representing the order lines to be saved.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> used to propagate notification that the operation should be canceled.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the list of saved <see cref="OrderLineEntity"/> instances if the operation succeeds,
    /// or an error result if the operation fails.
    /// </returns>
    public Task<Result<IReadOnlyCollection<OrderLineEntity>>> SaveOrderLinesAsync(IReadOnlyCollection<OrderLineEntity> lines,
        CancellationToken cancellationToken)
    {
        return SaveManyAsync(lines.Select(mapper.FromDomain).ToList(), cancellationToken).Map(_ => lines).MapError(error =>
        {
            return error switch
            {
                _ => CommerceErrors.FailedToSaveOrderLines
            };
        }).Map(_ => lines);
    }

}
