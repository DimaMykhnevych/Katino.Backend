using Katino.Domain.Entities;
using Katino.Domain.Enums.NovaPost;

namespace Katino.Domain.Models.NovaPost;

public class CreateNovaPostInternetDocument
{
    public string SenderCityRef { get; set; }
    public string SenderCounterpartyRef { get; set; }
    public string SenderContactPersonRef { get; set; }
    public string SenderContactPersonPhones { get; set; }
    public string SenderWarehouseIndex { get; set; }
    public string SenderWarehouseRef { get; set; }

    public string RecipientCityRef { get; set; }
    public string RecipientCounterpartyRef { get; set; }
    public string RecipientContactPersonRef { get; set; }
    public string RecipientPhone { get; set; }
    public string RecipientWarehouseIndex { get; set; }
    public string RecipientWarehouseRef { get; set; }
    public string RecipientFirstName { get; set; }
    public string RecipientMiddleName { get; set; }
    public string RecipientLastName { get; set; }

    public DeliveryType DeliveryType { get; set; }
    public PayerType PayerType { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public double Weight { get; set; }
    public int SeatsAmount { get; set; }
    public string Description { get; set; }
    public double Cost { get; set; }
    public double? AfterpaymentOnGoodsCost { get; set; }
    public IEnumerable<NpOptionsSeat> OptionsSeat { get; set; }


    // Address delivery properties
    public string RecipientAddressNote { get; set; }
    public string RecipientCityName { get; set; }
    public string RecipientAddressName { get; set; }
    public string RecipientHouse { get; set; }
    public string RecipientFlat { get; set; }
}
