using AutoMapper;
using Katino.Application.DTOs.Discount;
using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Services.DiscountN.UpdateDiscountService;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.DiscountN.UpdateDiscount;

public class UpdateDiscountCommandHandler : IRequestHandler<UpdateDiscountCommand, DiscountDto>
{
    private readonly IUpdateDiscountService _updateDiscountService;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public UpdateDiscountCommandHandler(
        IUpdateDiscountService updateDiscountService,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _updateDiscountService = updateDiscountService;
        _logger = loggerFactory?.CreateLogger(nameof(UpdateDiscountCommandHandler));
        _mapper = mapper;
    }

    public async Task<DiscountDto> Handle(UpdateDiscountCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling update discount request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var discountUpdate = new Discount
            {
                Id = request.Id,
                Name = request.Name,
                ValueType = _mapper.Map<DiscountValueType>(request.ValueType),
                Value = request.Value,
                IsActive = request.IsActive,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };

            var updated = await _updateDiscountService.UpdateAsync(
                discountUpdate, request.ProductIds, request.CollectionIds, request.BundleProductIds);

            return updated != null ? _mapper.Map<DiscountDto>(updated) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during updating discount");
            return null;
        }
    }
}
