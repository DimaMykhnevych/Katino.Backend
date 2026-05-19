using Katino.Domain.Models;

namespace Katino.Domain.Services.TelegramN;

public interface ISewingReportNotifier
{
    Task NotifyAsync(List<SewedReport> report, string sewerName);
}
