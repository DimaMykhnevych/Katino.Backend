namespace Katino.Application.DTOs.OrderAddressInfo;

public class UpdateOrderAddressInfoDto
{
    public Guid? Id { get; set; }
    public string RecipientAddressNote { get; set; }
    public string RecipientCity { get; set; }
    public string RecipientAddressName { get; set; }
    public string RecipientHouse { get; set; }
    public string RecipientFlat { get; set; }
}
