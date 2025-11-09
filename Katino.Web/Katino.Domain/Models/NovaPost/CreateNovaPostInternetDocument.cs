namespace Katino.Domain.Models.NovaPost;

public class CreateNovaPostInternetDocument
{
    public string SenderCityName { get; set; }
    public string SenderWarehouseId { get; set; }

    public string RecipientCityName { get; set; }
    public string RecipientWarehouseId { get; set; }
}
