using Katino.Domain.Repositories.DiscountRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.DiscountN.DeleteDiscount;

public class DeleteDiscountCommandHandler : IRequestHandler<DeleteDiscountCommand, bool>
{
    private readonly IDiscountRepository _discountRepository;
    private readonly ILogger _logger;

    public DeleteDiscountCommandHandler(
        IDiscountRepository discountRepository,
        ILoggerFactory loggerFactory)
    {
        _discountRepository = discountRepository;
        _logger = loggerFactory?.CreateLogger(nameof(DeleteDiscountCommandHandler));
    }

    public async Task<bool> Handle(DeleteDiscountCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling delete discount request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var discount = await _discountRepository.Get(request.Id);
            if (discount == null)
            {
                throw new ArgumentException($"Discount with id {request.Id} doesn't exist");
            }

            _discountRepository.Delete(discount);
            await _discountRepository.Save();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during deleting discount");
            return false;
        }
    }
}
