namespace Katino.Domain.Models.Pnl;

public class PnlRow
{
    public string Key { get; set; } = default!;
    public string Title { get; set; } = default!;
    public PnlRowKind Kind { get; set; }
    public decimal[] Months { get; set; } = new decimal[12];

    public decimal Total { get; set; }
    public decimal? SharePercent { get; set; }
}