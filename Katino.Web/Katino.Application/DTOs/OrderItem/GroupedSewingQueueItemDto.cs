namespace Katino.Application.DTOs.OrderItem;

public class GroupedSewingQueueItemDto
{
    public DateTime SendUntilDate { get; set; }
    public List<SewingQueueItemDto> Items { get; set; }
}
