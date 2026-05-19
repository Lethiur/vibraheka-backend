namespace VibraHeka.Domain.Ledger.Entities;

public class LedgerTransactionEntity : BaseAuditableEntity
{
    public string LedgerTransactionID { get; private set; } = string.Empty;

    public string SourceType { get; private set; } = default!;
    public string SourceID { get; private set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; private set; }

    public List<LedgerEntryEntity> Entries { get; private set; } = [];
}
