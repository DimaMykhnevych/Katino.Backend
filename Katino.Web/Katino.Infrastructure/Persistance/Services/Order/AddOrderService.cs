using Katino.Domain.Entities;
using Katino.Domain.Models;
using Katino.Domain.Models.NovaPost;
using Katino.Domain.Services.NovaPost.InternetDocument;
using Katino.Domain.Services.OrderN.AddOrderService;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Katino.Infrastructure.Persistance.Services.OrderN;

public class AddOrderService : IAddOrderService
{
    private readonly IInternetDocumentService _internetDocumentService;
    private readonly ILogger _logger;

    public AddOrderService(
        IInternetDocumentService internetDocumentService,
        ILoggerFactory loggerFactory)
    {
        _internetDocumentService = internetDocumentService;
        _logger = loggerFactory?.CreateLogger(nameof(AddOrderService));
    }

    public async Task<OrderCreationResult> AddAsync(Order order, CreateNovaPostInternetDocument document)
    {
        try
        {
            // Handling adding order


            // Handling creating internet document
            try
            {
                // !!!!!!!TODO firstly save this info in database - probably document and not the model of NP request!!!!!!!!!!

                var internetDocumentCreationResponse = await _internetDocumentService.CreateInternetDocumentAsync(document);
                if (!internetDocumentCreationResponse.Success)
                {
                    var response = JsonSerializer.Serialize(internetDocumentCreationResponse);
                    _logger.LogError($"An error occurred while creating internet document for order {order.Id}: {response}");
                    return new() { OrderAddedSuccessfully = true };
                }

                _logger.LogInformation($"Internet document for order {order.Id} created successfuly");

                // After successful creation update order with required info

                return new() { OrderAddedSuccessfully = true, NpInternetDocCreatedSuccessfully = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while adding NP internet document for order");
                return new() { OrderAddedSuccessfully = true };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while adding order");
            return new ();
        }
    }
}
