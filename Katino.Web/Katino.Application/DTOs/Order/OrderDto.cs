using Katino.Application.DTOs.NpCity;
using Katino.Application.DTOs.NpContactPerson;
using Katino.Application.DTOs.NpWarehouse;
using Katino.Application.DTOs.Order.NovaPost;
using Katino.Application.DTOs.OrderAddressInfo;
using Katino.Application.DTOs.OrderItem;
using Katino.Application.DTOs.OrderNpOptionsSeat;
using Katino.Application.DTOs.OrderRecipient;

namespace Katino.Application.DTOs.Order;

public class OrderDto
{
    public Guid Id { get; set; }
    public Guid SenderNpWarehouseId { get; set; }
    public Guid? RecipientNpWarehouseId { get; set; } // Nullable because for address delivery this field is null
    public Guid SenderNpCityId { get; set; }
    public Guid? RecipientNpCityId { get; set; } // Nullable because for address delivery this field is null
    public Guid SenderContactPersonId { get; set; }
    public Guid OrderRecipientId { get; set; }
    public PayerTypeDto PayerType { get; set; }
    public PaymentMethodDto PaymentMethod { get; set; }
    public SaleTypeDto SaleType { get; set; }
    public DateTime CreationDateTime { get; set; }
    public DateTime SendUntilDate { get; set; }
    public double Weight { get; set; }
    public DeliveryTypeDto DeliveryType { get; set; }
    public int SeatsAmount { get; set; }
    public string Description { get; set; }
    public double Cost { get; set; }
    public double? AfterpaymentOnGoodsCost { get; set; }
    public bool InternetDocumentCreationAttempted { get; set; }
    public string InternetDocumentRef { get; set; }
    public string InternetDocumentIntDocNumber { get; set; }
    public OrderStatusDto OrderStatus { get; set; }
    public OrderInternetDocStatusDto OrderInternetDocStatus { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = [];
    public List<OrderNpOptionsSeatDto> OrderNpOptionsSeats { get; set; } = [];

    public NpWarehouseDto SenderNpWarehouse { get; set; }
    public NpWarehouseDto RecipientNpWarehouse { get; set; }
    public GetNpCityDto SenderNpCity { get; set; }
    public GetNpCityDto RecipientNpCity { get; set; }
    public NpContactPersonDto SenderContactPerson { get; set; }
    public OrderRecipientDto OrderRecipient { get; set; }
    public OrderAddressInfoDto AddressInfo { get; set; }
}
