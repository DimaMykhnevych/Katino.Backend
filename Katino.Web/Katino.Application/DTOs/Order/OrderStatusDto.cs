namespace Katino.Application.DTOs.Order;

public enum OrderStatusDto
{
    None = 0,

    InProgress = 1,
    ReadyToShip = 2,
    Packed = 3,

    Created = 4,

    Deleted = 5,

    NotFound = 6,

    InTheCityInterregional = 7,

    OnTheWayToCity = 8,

    OnTheWayToDepartment = 9,

    Arrived = 10,

    ArrivedPostomat = 11,

    Received = 12,

    ReceivedRemittancePending = 13,

    ReceivedRemittanceCompleted = 14,

    NpCompletingOrder = 15,

    InTheCityWithinTheCity = 16,

    OnTheWayToReceiver = 17,

    RejectionBySender = 18,

    Rejection = 19,

    AddressChanged = 20,

    StorageStopped = 21,

    ReceivedAndReturnCreated = 22,

    ReceiverNotAnswering = 23,

    DeliveryDateChangedByReceiver = 24,

    Refusal = 200,
    Exchange = 201,
}
