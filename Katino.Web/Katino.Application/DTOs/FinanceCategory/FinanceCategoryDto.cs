 namespace Katino.Application.DTOs.FinanceCategory;

public class FinanceCategoryDto
{
    public Guid Id { get; set; }
    public FinanceCategoryTypeDto Type { get; set; }
    public string Name { get; set; } = default!;
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}
