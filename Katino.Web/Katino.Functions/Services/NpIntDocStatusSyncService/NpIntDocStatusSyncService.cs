using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Repositories.OrderRepository;
using Katino.Domain.Services.NovaPost.InternetDocument;
using Microsoft.Extensions.Logging;

namespace Katino.Functions.Services.NpIntDocStatusSyncService;

public class NpIntDocStatusSyncService : INpIntDocStatusSyncService
{
    private const int OrderBatchSize = 10;

    private readonly IInternetDocumentService _internetDocumentService;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger _logger;

    // TODO move to constants in Domain and separate into two lists - received statuses and rejected
    private readonly OrderInternetDocStatus[] excludedStatuses =
    [
        OrderInternetDocStatus.Received,                    // 9
        OrderInternetDocStatus.RejectionBySender,           // 102
        OrderInternetDocStatus.Rejection,                   // 103
        OrderInternetDocStatus.StorageStopped,              // 105
        OrderInternetDocStatus.ReceivedAndReturnCreated,    // 106
        OrderInternetDocStatus.ReceiverNotAnswering         // 111
    ];

    public NpIntDocStatusSyncService(
        IInternetDocumentService internetDocumentService,
        IOrderRepository orderRepository,
        ILoggerFactory loggerFactory)
    {
        _internetDocumentService = internetDocumentService;
        _orderRepository = orderRepository;
        _logger = loggerFactory?.CreateLogger(nameof(NpIntDocStatusSyncService));
    }

    public async Task RunSync()
    {
        var ordersToCheck = await _orderRepository.GetOrdersForNpStatusUpdateAsync(excludedStatuses);

        var batches = ordersToCheck.Chunk(10);

        foreach (var batch in batches)
        {
            await ProccessOrderBatch(batch);
        }

        await _orderRepository.Save();
    }

    private async Task ProccessOrderBatch(Order[] batch)
    {
        try
        {
            var orderIntDocNumbers = batch.Select(o => o.InternetDocumentIntDocNumber).ToList();
            var statuses = await _internetDocumentService.GetIntDocStatuses(orderIntDocNumbers);
            var statusesDict = statuses.ToDictionary(k => k["Number"].ToString(), v => v["StatusCode"].ToString());
            foreach (var order in batch)
            {
                await ProcessOrder(order, statusesDict);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during batch processing");
        }
    }

    private async Task ProcessOrder(Order order, Dictionary<string, string> statusesDict)
    {
        try
        {
            var orderStatus = statusesDict[order.InternetDocumentIntDocNumber];
            order.OrderInternetDocStatus = (OrderInternetDocStatus)int.Parse(orderStatus);
            await _orderRepository.Update(order);

            // TODO handle decline status
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred during processing of order {order.Id}");
        }
    }
}
