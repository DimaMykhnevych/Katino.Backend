using Katino.Domain.Enums;

namespace Katino.Domain.Constants;

public static class InternetDocumentConstants
{
    public static readonly OrderStatus[] OrderReceivedStatuses = [
        OrderStatus.Received // 9
    ];

    public static readonly OrderStatus[] OrderRejectedStatuses = [
        OrderStatus.RejectionBySender,           // 102
        OrderStatus.Rejection,                   // 103
        OrderStatus.StorageStopped,              // 105
        OrderStatus.ReceivedAndReturnCreated,    // 106
        OrderStatus.ReceiverNotAnswering         // 111
    ];
}
