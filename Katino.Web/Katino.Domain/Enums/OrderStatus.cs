namespace Katino.Domain.Enums;

public enum OrderStatus
{
    None = 0,

    InProgress = 1,
    ReadyToShip = 2,
    Packed = 3,

    // NP-related statuses

    /// <summary>
    /// The sender created this Internet doc independently, but has not yet submitted it for shipment.
    /// </summary>
    Created = 4,

    /// <summary>
    /// Int doc deleted.
    /// </summary>
    Deleted = 5,

    /// <summary>
    /// Int doc not found.
    /// </summary>
    NotFound = 6,

    /// <summary>
    /// Departure in the city of XXXX. (Status for interregional shipments)
    /// </summary>
    InTheCityInterregional = 7,

    /// <summary>
    /// The shipment is headed to the city of YYYY.
    /// </summary>
    OnTheWayToCity = 8,

    /// <summary>
    /// Departure from YYYY, estimated delivery to DEPARTMENT-XXX dd-mm. Expect additional arrival notification.
    /// </summary>
    OnTheWayToDepartment = 9,

    /// <summary>
    /// Arrived at the NP department.
    /// </summary>
    Arrived = 10,

    /// <summary>
    /// Arrived at the department (loaded into the Postomat).
    /// </summary>
    ArrivedPostomat = 11,

    /// <summary>
    /// Shipment received.
    /// </summary>
    Received = 12,

    /// <summary>
    /// The shipment was received on %DateReceived%. Within 24 hours you will receive an SMS
    /// notification about the receipt of the money transfer and you can pick it up at the Nova Poshta branch cash desk.
    /// </summary>
    ReceivedRemittancePending = 13,

    /// <summary>
    /// Shipment received on %DateReceived%. Money order issued to recipient.
    /// </summary>
    ReceivedRemittanceCompleted = 14,

    /// <summary>
    /// Nova Poshta completes your shipment.
    /// </summary>
    NpCompletingOrder = 15,

    /// <summary>
    /// Departure in the city of XXXX. (Status for local standard and local express services - delivery within the city)
    /// </summary>
    InTheCityWithinTheCity = 16,

    /// <summary>
    /// On the way to the recipient.
    /// </summary>
    OnTheWayToReceiver = 17,

    /// <summary>
    /// Rejection (Sender created a return order).
    /// </summary>
    RejectionBySender = 18,

    /// <summary>
    /// Refusal to receive.
    /// </summary>
    Rejection = 19,

    /// <summary>
    /// Address changed.
    /// </summary>
    AddressChanged = 20,

    /// <summary>
    /// Storage stopped.
    /// </summary>
    StorageStopped = 21,

    /// <summary>
    /// Received and return delivery note created.
    /// </summary>
    ReceivedAndReturnCreated = 22,

    /// <summary>
    /// Unsuccessful delivery attempt due to the Recipient not being at the address or unable to contact him/her.
    /// </summary>
    ReceiverNotAnswering = 23,

    /// <summary>
    /// Delivery date postponed by Recipient.
    /// </summary>
    DeliveryDateChangedByReceiver = 24,


    // Other manual statuses
    Refusal = 200,
    Exchange = 201,
}
