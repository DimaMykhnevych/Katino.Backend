using Katino.Domain.Entities;
using Katino.Domain.Repositories.OrderRecipientRepository;
using Katino.Domain.Services.OrderRecipientN.AddOrderRecipientService;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.OrderRecipientN;

public class AddOrderRecipientService : IAddOrderRecipientService
{
    private readonly IOrderRecipientRepository _orderRecipientRepository;
    private readonly ILogger _logger;

    public AddOrderRecipientService(
        IOrderRecipientRepository orderRecipientRepository,
        ILoggerFactory loggerFactory)
    {
        _orderRecipientRepository = orderRecipientRepository;
        _logger = loggerFactory?.CreateLogger(nameof(AddOrderRecipientService));
    }

    public async Task<Guid> UpsertOrderRecipientAsync(OrderRecipient orderRecipient)
    {
        var existingOrderRecipient = await _orderRecipientRepository
            .GetOrderRecipientByPhoneNumberAsync(orderRecipient.NpContactPerson.Phones);

        if (existingOrderRecipient == null)
        {
            _logger.LogDebug($"Order recipient {orderRecipient.NpContactPerson.Phones} doesn't exist in database, adding...");
            orderRecipient.CreatedDate = DateTime.UtcNow;
            var addedOrderRecipient = await _orderRecipientRepository.Insert(orderRecipient);
            await _orderRecipientRepository.Save();
            return addedOrderRecipient.Id;
        }

        _logger.LogDebug($"Order recipient {orderRecipient.NpContactPerson.Phones} exists in database, updating...");

        existingOrderRecipient.InstUrl = orderRecipient.InstUrl;

        existingOrderRecipient.NpContactPerson.LastName = orderRecipient.NpContactPerson.LastName;
        existingOrderRecipient.NpContactPerson.FirstName = orderRecipient.NpContactPerson.FirstName;
        existingOrderRecipient.NpContactPerson.MiddleName = orderRecipient.NpContactPerson.MiddleName;
        existingOrderRecipient.NpContactPerson.Phones = orderRecipient.NpContactPerson.Phones;

        await _orderRecipientRepository.Update(existingOrderRecipient);
        await _orderRecipientRepository.Save();
        return existingOrderRecipient.Id;
    }
}
