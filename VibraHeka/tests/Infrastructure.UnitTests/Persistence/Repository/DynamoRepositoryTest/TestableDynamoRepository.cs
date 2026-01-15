using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Configuration;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.DynamoRepositoryTest;

public class TestEntity
{
    public string ID { get; set; } = string.Empty;
}

public class TestableDynamoRepository(IDynamoDBContext context, IConfiguration config, string key)
    : GenericDynamoRepository<TestEntity>(context, config, key)
{
    public Task<Result<TestEntity>> ExposedFindByID(string id) => FindByID(id);
    public Task<Result<Unit>> ExposedSave(TestEntity entity) => Save(entity);
    protected override string HandleError(Exception ex) => $"Handled: {ex.Message}";
}
