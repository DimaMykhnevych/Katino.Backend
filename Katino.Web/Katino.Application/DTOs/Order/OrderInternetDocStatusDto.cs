namespace Katino.Application.DTOs.Order;

public enum OrderInternetDocStatusDto
{
    NotProcessed = 0,

    Created = 1,

    Deleted = 2,

    NotFound = 3,

    InTheCityInterregional = 4,

    OnTheWayToCity = 5,

    OnTheWayToDepartment = 6,

    Arrived = 7,

    ArrivedPostomat = 8,

    Received = 9,

    ReceivedRemittancePending = 10,

    ReceivedRemittanceCompleted = 11,

    NpCompletingOrder = 12,

    InTheCityWithinTheCity = 41,

    OnTheWayToReceiver = 101,

    RejectionBySender = 102,

    Rejection = 103,

    AddressChanged = 104,

    StorageStopped = 105,

    ReceivedAndReturnCreated = 106,

    ReceiverNotAnswering = 111,

    DeliveryDateChangedByReceiver = 112

}
