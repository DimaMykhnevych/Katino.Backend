using Katino.Application.DTOs.Telegram;
using MediatR;

namespace Katino.Application.Queries.Telegram.GetTelegramChats;

public class GetTelegramChatsQuery : IRequest<List<TelegramChatDto>>
{
    public string Token { get; set; }
}
