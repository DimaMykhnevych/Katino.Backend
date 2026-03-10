using Katino.Domain.Enums;
using Katino.Domain.Helpers;
using Katino.Domain.Repositories.OrderRepository;
using Katino.Domain.Services.NovaPost.InternetDocument;
using Katino.Domain.Services.OrderN.CreateOrdersScanSheetService;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.OrderN;

public class CreateOrdersScanSheetService : ICreateOrdersScanSheetService
{
    private readonly IInternetDocumentService _internetDocumentService;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger _logger;

    public CreateOrdersScanSheetService(
        IInternetDocumentService internetDocumentService,
        IOrderRepository orderRepository,
        ILoggerFactory loggerFactory)
    {
        _internetDocumentService = internetDocumentService;
        _orderRepository = orderRepository;
        _logger = loggerFactory?.CreateLogger(nameof(CreateOrdersScanSheetService));
    }

    public async Task<bool> CreateOrdersScanSheetAsync()
    {
        _logger.LogInformation($"Creating order scan sheet");

        var readyOrdersRefs = await _orderRepository.GetOrderRefsWithStatusAndInternetDocCreatedAsync(OrderStatus.Packed);

        _logger.LogInformation($"Order refs count to create scan sheet: {readyOrdersRefs.Count}");
        if (readyOrdersRefs.Count == 0)
        {
            _logger.LogInformation("There aren't any packed orders, scan sheet is not created");
            return true;
        }

        var currentTime = DateTimeHelper.GetCurrentKyivDateTime().ToString("dd.MM");

        var creationResponse = await _internetDocumentService.CreateScanSheet(readyOrdersRefs, currentTime);
        if (creationResponse == null || creationResponse.Errors.Any())
        {
            _logger.LogError("An error occurred during scan sheet creation");
            return false;
        }

        return true;
    }
}
