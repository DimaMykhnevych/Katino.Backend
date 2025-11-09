namespace Katino.Domain.Models.NovaPost;

public class NpInternetDocumentCreationResponse
{
    public string Ref { get; set; }
    public string CostOnSite { get; set; }
    public string IntDocNumber { get; set; }
    public string TypeDocument { get; set; }
    public DateOnly EstimatedDeliveryDate { get; set; }
}
