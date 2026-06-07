using Katino.Domain.Repositories.DiscountRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.DiscountN.SetDiscountActive;

public class SetDiscountActiveCommandHandler : IRequestHandler<SetDiscountActiveCommand, bool>
{
    private readonly IDiscountRepository _discountRepository;
    private readonly ILogger _logger;

    public SetDiscountActiveCommandHandler(
        IDiscountRepository discountRepository,
        ILoggerFactory loggerFactory)
    {
        _discountRepository = discountRepository;
        _logger = loggerFactory?.CreateLogger(nameof(SetDiscountActiveCommandHandler));
    }

    public async Task<bool> Handle(SetDiscountActiveCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling set discount active request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var discount = await _discountRepository.Get(request.Id);
            if (discount == null)
            {
                _logger.LogWarning("Discount {DiscountId} not found", request.Id);
                return false;
            }

            discount.IsActive = request.IsActive;
            await _discountRepository.Update(discount);
            await _discountRepository.Save();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during setting discount active status");
            return false;
        }
    }
}
