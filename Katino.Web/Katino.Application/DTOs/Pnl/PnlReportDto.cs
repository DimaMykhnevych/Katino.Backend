namespace Katino.Application.DTOs.Pnl;

public class PnlReportDto
{
    public int Year { get; set; }
    public List<PnlRowDto> Rows { get; set; } = new ();
}
