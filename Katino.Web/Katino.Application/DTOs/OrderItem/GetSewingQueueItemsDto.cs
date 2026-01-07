namespace Katino.Application.DTOs.OrderItem;

public class GetSewingQueueItemsDto
{
    public IEnumerable<SewingQueueItemDto> SewingQueueItems { get; set; }
    public int ResultsAmount { get; set; }
}
