using MediatR;

namespace Katino.Application.Commands.PayOrder;

public class PayOrderCommandHandler : IRequestHandler<PayOrderCommand, bool>
{
    public PayOrderCommandHandler()
    {

    }

    public async Task<bool> Handle(PayOrderCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
