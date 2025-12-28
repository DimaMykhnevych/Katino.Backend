using Katino.Domain.Enums;
using Katino.Domain.Enums.NovaPost;

namespace Katino.Domain.Entities;

// ON ADDING NEW PROPERTIES ALSO ADD THEM IN UPDATEORDERSERVICE
public class Order
{
    public Guid Id { get; set; }
    public Guid SenderNpWarehouseId { get; set; }
    public Guid? RecipientNpWarehouseId { get; set; } // Nullable because for address delivery this field is null
    public Guid SenderNpCityId { get; set; }
    public Guid? RecipientNpCityId { get; set; } // Nullable because for address delivery this field is null
    public Guid SenderContactPersonId { get; set; }
    public Guid OrderRecipientId { get; set; }
    public PayerType PayerType { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public SaleType SaleType { get; set; }
    public DateTime CreationDateTime { get; set; }
    public DateTime SendUntilDate { get; set; }
    public double Weight { get; set; }
    public DeliveryType DeliveryType { get; set; }
    public int SeatsAmount { get; set; }
    public string Description { get; set; }
    public double Cost { get; set; }
    public double? AfterpaymentOnGoodsCost { get; set; }

    // InternetDocument Creation Result
    public bool InternetDocumentCreationAttempted { get; set; }
    public string InternetDocumentRef { get; set; }
    public string InternetDocumentIntDocNumber { get; set; }

    // Internally calculated fields
    public OrderReadinessStatus OrderReadinessStatus { get; set; }
    public OrderInternetDocStatus OrderInternetDocStatus { get; set; }
    public OrderManualStatus OrderManualStatus { get; set; }


    public List<OrderItem> OrderItems { get; set; } = [];
    public List<OrderNpOptionsSeat> OrderNpOptionsSeats { get; set; } = [];


    public NpWarehouse SenderNpWarehouse { get; set; }
    public NpWarehouse RecipientNpWarehouse { get; set; }
    public NpCity SenderNpCity { get; set; }
    public NpCity RecipientNpCity { get; set; }
    public NpContactPerson SenderContactPerson { get; set; }
    public OrderRecipient OrderRecipient { get; set; }
    public OrderAddressInfo AddressInfo { get; set; }
}
