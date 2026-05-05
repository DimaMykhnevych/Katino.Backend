using MediatR;

namespace Katino.Application.Queries.FinanceEntryN.GetPnlScreenshot;

public class GetPnlScreenshotQuery : IRequest<(Stream Content, string ContentType)>
{
    public int Year { get; set; }
}
