namespace Katino.Domain.Models;

public class OrderCreationResult
{
    public bool OrderAddedSuccessfully { get; set; }
    public bool NpInternetDocCreatedSuccessfully { get; set; }
    public decimal CalculatedCost { get; set; }
}
