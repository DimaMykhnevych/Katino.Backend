using Katino.Application.DTOs.Telegram;
using MediatR;

namespace Katino.Application.Queries.Telegram.GetTelegramSettings;

public class GetTelegramSettingsQuery : IRequest<TelegramSettingsDto>
{
}
