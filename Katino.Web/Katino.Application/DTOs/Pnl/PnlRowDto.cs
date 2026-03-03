namespace Katino.Application.DTOs.Pnl;

public class PnlRowDto
{
    public string Key { get; set; } = default!;
    public string Title { get; set; } = default!;
    public PnlRowKindDto Kind { get; set; }
    public decimal[] Months { get; set; } = new decimal[12];

    public decimal Total { get; set; }
    public decimal? SharePercent { get; set; }
}