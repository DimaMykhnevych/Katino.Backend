using Katino.Domain.Enums;

namespace Katino.Domain.Entities;

public class FinanceCategory
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;
    public FinanceCategoryType Type { get; set; }

    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;

    public List<FinanceEntry> Entries { get; set; } = new();
}
