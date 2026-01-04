namespace Katino.Application.DTOs.Order;

public class GetOrderDto
{
    public IEnumerable<OrderDto> Orders { get; set; }
    public int ResultsAmount { get; set; }
}
