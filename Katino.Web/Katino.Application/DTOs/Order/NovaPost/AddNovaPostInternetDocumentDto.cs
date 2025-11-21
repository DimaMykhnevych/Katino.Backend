namespace Katino.Application.DTOs.Order.NovaPost;

public class AddNovaPostInternetDocumentDto
{
    public string SenderCityName { get; set; }
    public string SenderWarehouseId { get; set; }

    public string RecipientCityName { get; set; }
    public string RecipientWarehouseId { get; set; }

    public PayerTypeDto PayerType { get; set; }
    public PaymentMethodDto PaymentMethod { get; set; }
    public double Weight { get; set; }
    public int SeatsAmount { get; set; }
    public string Description { get; set; }
    public double Cost { get; set; }
    public string RecipientFirstName { get; set; }
    public string RecipientMiddleName { get; set; }
    public string RecipientLastName { get; set; }
    public string RecipientPhone { get; set; }
    public IEnumerable<OptionsSeatDto> OptionsSeat { get; set; }

    public double? AfterpaymentOnGoodsCost { get; set; }

    // Address delivery properties
    public string RecipientAddressNote { get; set; }
    public string RecipientAddressName { get; set; }
    public string RecipientHouse { get; set; }
    public string RecipientFlat { get; set; }


    // Non NovaPost required properties
    public DeliveryTypeDto DeliveryType { get; set; }
}
