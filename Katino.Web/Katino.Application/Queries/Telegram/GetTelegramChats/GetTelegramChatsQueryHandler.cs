using Katino.Application.DTOs.Telegram;
using Katino.Domain.Services.TelegramN;
using MediatR;

namespace Katino.Application.Queries.Telegram.GetTelegramChats;

public class GetTelegramChatsQueryHandler : IRequestHandler<GetTelegramChatsQuery, List<TelegramChatDto>>
{
    private readonly ITelegramService _telegramService;

    public GetTelegramChatsQueryHandler(ITelegramService telegramService)
    {
        _telegramService = telegramService;
    }

    public async Task<List<TelegramChatDto>> Handle(GetTelegramChatsQuery request, CancellationToken cancellationToken)
    {
        var chats = await _telegramService.GetAvailableChatsAsync(request.Token);

        return chats.Select(c => new TelegramChatDto
        {
            Id = c.Id,
            Title = c.Title,
            Type = c.Type
        }).ToList();
    }
}
