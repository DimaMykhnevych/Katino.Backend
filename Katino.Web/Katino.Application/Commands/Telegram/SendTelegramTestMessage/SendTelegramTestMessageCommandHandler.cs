using Katino.Domain.Services.TelegramN;
using MediatR;

namespace Katino.Application.Commands.Telegram.SendTelegramTestMessage;

public class SendTelegramTestMessageCommandHandler : IRequestHandler<SendTelegramTestMessageCommand, bool>
{
    private readonly ITelegramService _telegramService;

    public SendTelegramTestMessageCommandHandler(ITelegramService telegramService)
    {
        _telegramService = telegramService;
    }

    public async Task<bool> Handle(SendTelegramTestMessageCommand request, CancellationToken cancellationToken)
    {
        await _telegramService.SendTestMessageAsync();
        return true;
    }
}
