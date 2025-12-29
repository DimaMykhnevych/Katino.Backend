using Katino.Domain.Enums;

namespace Katino.Domain.Constants;

public static class InternetDocumentConstants
{
    public static readonly OrderInternetDocStatus[] ReceivedStatuses = [
        OrderInternetDocStatus.Received // 9
    ];

    public static readonly OrderInternetDocStatus[] RejectedStatuses = [
        OrderInternetDocStatus.RejectionBySender,           // 102
        OrderInternetDocStatus.Rejection,                   // 103
        OrderInternetDocStatus.StorageStopped,              // 105
        OrderInternetDocStatus.ReceivedAndReturnCreated,    // 106
        OrderInternetDocStatus.ReceiverNotAnswering         // 111
    ];
}
