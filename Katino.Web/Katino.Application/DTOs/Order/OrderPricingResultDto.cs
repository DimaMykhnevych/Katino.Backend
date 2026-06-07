namespace Katino.Application.DTOs.Order;

public class OrderPricingResultDto
{
    public decimal BaseTotal { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal FinalTotal { get; set; }
    public List<ItemPricingResultDto> ItemResults { get; set; } = [];
}
