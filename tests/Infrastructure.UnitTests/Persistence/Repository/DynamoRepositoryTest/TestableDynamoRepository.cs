using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.DynamoRepositoryTest;

public class TestEntity
{
    public string ID { get; set; } = string.Empty;
}

public class TestableDynamoRepository(IDynamoDBContext context, string key)
    : GenericDynamoRepository<TestEntity>(context, key, LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<GenericDynamoRepository<TestEntity>>())
{
    public Task<Result<TestEntity>> ExposedFindByID(string id) => FindByID(id, CancellationToken.None);
    public Task<Result<Unit>> ExposedSave(TestEntity entity) => Save(entity);
}
