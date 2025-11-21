using Katino.Domain.Enums.NovaPost;

namespace Katino.Domain.Models.NovaPost;

public class CreateNovaPostInternetDocument
{
    public string SenderCityName { get; set; }
    public string SenderWarehouseId { get; set; }

    public string RecipientCityName { get; set; }
    public string RecipientWarehouseId { get; set; }

    public PayerType PayerType { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public double Weight { get; set; }
    public int SeatsAmount { get; set; }
    public string Description { get; set; }
    public double Cost { get; set; }
    public string RecipientFirstName { get; set; }
    public string RecipientMiddleName { get; set; }
    public string RecipientLastName { get; set; }
    public string RecipientPhone { get; set; }
    public IEnumerable<OptionsSeat> OptionsSeat { get; set; }

    public double? AfterpaymentOnGoodsCost { get; set; }

    // Address delivery properties
    public string RecipientAddressNote { get; set; }
    public string RecipientAddressName { get; set; }
    public string RecipientHouse { get; set; }
    public string RecipientFlat { get; set; }

    // Non NovaPost required properties
    public DeliveryType DeliveryType { get; set; }
}
