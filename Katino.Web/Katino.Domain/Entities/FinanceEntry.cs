using Katino.Domain.Enums;

namespace Katino.Domain.Entities;

public class FinanceEntry
{
    public Guid Id { get; set; }
    public DateTime EntryDate { get; set; }
    public decimal Amount { get; set; }
    public string? Comment { get; set; }
    public FinanceEntrySourceType SourceType { get; set; }
    public FinanceEntryReason Reason { get; set; }
    public SaleType? SaleType { get; set; }
    public bool IsLocked { get; set; } = false;
    public string? InternetDocumentIntDocNumber { get; set; }

    public Guid CategoryId { get; set; }
    public FinanceCategory Category { get; set; } = default!;

    public Guid? OrderId { get; set; }
    public Order? Order { get; set; }

    public Guid? CreatedBy { get; set; }
    public AppUser? CreatedByUser { get; set; }

    public Guid? ReversedEntryId { get; set; }
    public FinanceEntry? ReversedEntry { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
