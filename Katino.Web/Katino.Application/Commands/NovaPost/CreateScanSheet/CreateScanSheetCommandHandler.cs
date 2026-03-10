using Katino.Domain.Services.OrderN.CreateOrdersScanSheetService;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.NovaPost.CreateScanSheet;

public class CreateScanSheetCommandHandler : IRequestHandler<CreateScanSheetCommand, bool>
{
    private readonly ICreateOrdersScanSheetService _createOrdersScanSheetService;
    private readonly ILogger _logger;

    public CreateScanSheetCommandHandler(
        ICreateOrdersScanSheetService createOrdersScanSheetService,
        ILoggerFactory loggerFactory)
    {
        _createOrdersScanSheetService = createOrdersScanSheetService;
        _logger = loggerFactory?.CreateLogger(nameof(CreateScanSheetCommandHandler));
    }

    public async Task<bool> Handle(CreateScanSheetCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling create scan sheet request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _createOrdersScanSheetService.CreateOrdersScanSheetAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during creating scan sheet request");
            return false;
        }
    }
}
