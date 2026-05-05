using Katino.Domain.Services.AzureStorage;
using MediatR;

namespace Katino.Application.Queries.FinanceEntryN.GetPnlScreenshot;

public class GetPnlScreenshotQueryHandler : IRequestHandler<GetPnlScreenshotQuery, (Stream Content, string ContentType)>
{
    private static readonly IReadOnlyCollection<int> AvailableYears = [2023, 2024, 2025];

    private readonly IAzureStorageService _storageService;

    public GetPnlScreenshotQueryHandler(IAzureStorageService storageService)
    {
        _storageService = storageService;
    }

    public async Task<(Stream Content, string ContentType)> Handle(GetPnlScreenshotQuery request, CancellationToken cancellationToken)
    {
        if (!AvailableYears.Contains(request.Year))
            throw new ArgumentOutOfRangeException(nameof(request.Year), "No screenshot available for the requested year.");

        return await _storageService.GetFinanceReportScreenshotAsync(request.Year);
    }
}
