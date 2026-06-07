namespace Katino.Domain.Models;

public class OrderUpdateResult
{
    public bool OrderUpdatedSuccessfully { get; set; }
    public bool NpInternetDocUpdatedSuccessfully { get; set; }
    public decimal CalculatedCost { get; set; }
}
