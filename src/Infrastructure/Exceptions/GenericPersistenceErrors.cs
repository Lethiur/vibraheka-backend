namespace VibraHeka.Infrastructure.Exceptions;

public class GenericPersistenceErrors
{
    public const string NoRecordsFound = "GPE-000";
    public const string ResourceNotFound = "GPE-001";
    public const string ProvisionedThroughputExceeded = "GPE-002";
    public const string ConditionalCheckFailed = "GPE-003";
    public const string GeneralError = "GPE-999";
}
