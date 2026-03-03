namespace Katino.Domain.Models.Pnl;

public class PnlReport
{
    public int Year { get; set; }
    public List<PnlRow> Rows { get; set; } = new();
}
