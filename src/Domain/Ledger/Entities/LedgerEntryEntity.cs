using NMoneys;

namespace VibraHeka.Domain.Ledger.Entities;

public class LedgerEntryEntity : BaseAuditableEntity
{
    public string LedgerEntryID { get; private set; } = string.Empty;

    public string LedgerTransactionId { get; private set; } = string.Empty;

    public string AccountCode { get; private set; } = string.Empty;

    public Money Debit { get; private set; }
    public Money Credit { get; private set; }
}
