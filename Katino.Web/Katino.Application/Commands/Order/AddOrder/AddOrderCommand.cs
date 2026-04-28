using Katino.Application.DTOs.NpCity;
using Katino.Application.DTOs.NpContactPerson;
using Katino.Application.DTOs.Order;
using Katino.Application.DTOs.Order.NovaPost;
using Katino.Application.DTOs.OrderAddressInfo;
using Katino.Application.DTOs.OrderItem;
using Katino.Application.DTOs.OrderNpOptionsSeat;
using Katino.Application.DTOs.OrderRecipient;
using MediatR;

namespace Katino.Application.Commands.OrderN.AddOrder;

public class AddOrderCommand : IRequest<OrderCreationResultDto>
{
    public Guid SenderNpWarehouseId { get; set; }
    public Guid? RecipientNpWarehouseId { get; set; }
    public PayerTypeDto PayerType { get; set; }
    public PaymentMethodDto PaymentMethod { get; set; }
    public SaleTypeDto SaleType { get; set; }
    public DateTime SendUntilDate { get; set; }
    public double Weight { get; set; }
    public DeliveryTypeDto DeliveryType { get; set; }
    public int SeatsAmount { get; set; }
    public string Description { get; set; }
    public double Cost { get; set; }
    public double? AfterpaymentOnGoodsCost { get; set; }
    public string Comment { get; set; }

    public List<AddOrderItemDto> OrderItems { get; set; } = [];
    public List<AddOrderNpOptionsSeatDto> OrderNpOptionsSeats { get; set; } = [];
    public List<string> CustomTags { get; set; } = [];

    public AddNpCityDto SenderNpCity { get; set; }
    public AddNpCityDto RecipientNpCity { get; set; }
    public AddNpContactPersonDto SenderContactPerson { get; set; }
    public AddOrderRecipientDto OrderRecipient { get; set; }
    public AddOrderAddressInfoDto AddressInfo { get; set; }
    public string GeneralOrderInfo { get; set; }
}
