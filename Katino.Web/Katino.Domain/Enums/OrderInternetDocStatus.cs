namespace Katino.Domain.Enums;

public enum OrderInternetDocStatus
{
    NotProcessed = 0,

    /// <summary>
    /// The sender created this Internet doc independently, but has not yet submitted it for shipment.
    /// </summary>
    Created = 1,

    /// <summary>
    /// Int doc deleted.
    /// </summary>
    Deleted = 2,

    /// <summary>
    /// Int doc not found.
    /// </summary>
    NotFound = 3,

    /// <summary>
    /// Departure in the city of XXXX. (Status for interregional shipments)
    /// </summary>
    InTheCityInterregional = 4,

    /// <summary>
    /// The shipment is headed to the city of YYYY.
    /// </summary>
    OnTheWayToCity = 5,

    /// <summary>
    /// Departure from YYYY, estimated delivery to DEPARTMENT-XXX dd-mm. Expect additional arrival notification.
    /// </summary>
    OnTheWayToDepartment = 6,

    /// <summary>
    /// Arrived at the NP department.
    /// </summary>
    Arrived = 7,

    /// <summary>
    /// Arrived at the department (loaded into the Postomat).
    /// </summary>
    ArrivedPostomat = 8,

    /// <summary>
    /// Shipment received.
    /// </summary>
    Received = 9,

    /// <summary>
    /// The shipment was received on %DateReceived%. Within 24 hours you will receive an SMS
    /// notification about the receipt of the money transfer and you can pick it up at the Nova Poshta branch cash desk.
    /// </summary>
    ReceivedRemittancePending = 10,

    /// <summary>
    /// Shipment received on %DateReceived%. Money order issued to recipient.
    /// </summary>
    ReceivedRemittanceCompleted = 11,

    /// <summary>
    /// Nova Poshta completes your shipment.
    /// </summary>
    NpCompletingOrder = 12,

    /// <summary>
    /// Departure in the city of XXXX. (Status for local standard and local express services - delivery within the city)
    /// </summary>
    InTheCityWithinTheCity = 41,

    /// <summary>
    /// On the way to the recipient.
    /// </summary>
    OnTheWayToReceiver = 101,

    /// <summary>
    /// Rejection (Sender created a return order).
    /// </summary>
    RejectionBySender = 102,

    /// <summary>
    /// Refusal to receive.
    /// </summary>
    Rejection = 103,

    /// <summary>
    /// Address changed.
    /// </summary>
    AddressChanged = 104,

    /// <summary>
    /// Storage stopped.
    /// </summary>
    StorageStopped = 105,

    /// <summary>
    /// Received and return delivery note created.
    /// </summary>
    ReceivedAndReturnCreated = 106,

    /// <summary>
    /// Unsuccessful delivery attempt due to the Recipient not being at the address or unable to contact him/her.
    /// </summary>
    ReceiverNotAnswering = 111,

    /// <summary>
    /// Delivery date postponed by Recipient.
    /// </summary>
    DeliveryDateChangedByReceiver = 112
}
