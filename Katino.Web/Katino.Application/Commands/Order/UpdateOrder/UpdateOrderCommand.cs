using Katino.Application.DTOs.NpCity;
using Katino.Application.DTOs.NpContactPerson;
using Katino.Application.DTOs.Order;
using Katino.Application.DTOs.Order.NovaPost;
using Katino.Application.DTOs.OrderAddressInfo;
using Katino.Application.DTOs.OrderItem;
using Katino.Application.DTOs.OrderNpOptionsSeat;
using Katino.Application.DTOs.OrderRecipient;
using MediatR;

namespace Katino.Application.Commands.OrderN.UpdateOrder;

public class UpdateOrderCommand : IRequest<OrderUpdateResultDto>
{
    public Guid Id { get; set; }
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

    public List<UpdateOrderItemDto> OrderItems { get; set; } = [];
    public List<UpdateOrderNpOptionsSeatDto> OrderNpOptionsSeats { get; set; } = [];

    public UpdateNpCityDto SenderNpCity { get; set; }
    public UpdateNpCityDto RecipientNpCity { get; set; }
    public UpdateNpContactPersonDto SenderContactPerson { get; set; }
    public UpdateOrderRecipientDto OrderRecipient { get; set; }
    public UpdateOrderAddressInfoDto AddressInfo { get; set; }
}
