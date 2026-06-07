using AutoMapper;
using Katino.Application.DTOs.Discount;
using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Services.DiscountN.AddDiscountService;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.DiscountN.AddDiscount;

public class AddDiscountCommandHandler : IRequestHandler<AddDiscountCommand, DiscountDto>
{
    private readonly IAddDiscountService _addDiscountService;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public AddDiscountCommandHandler(
        IAddDiscountService addDiscountService,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _addDiscountService = addDiscountService;
        _logger = loggerFactory?.CreateLogger(nameof(AddDiscountCommandHandler));
        _mapper = mapper;
    }

    public async Task<DiscountDto> Handle(AddDiscountCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling add discount request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var discountType = _mapper.Map<DiscountType>(request.Type);

            var discount = new Discount
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Type = discountType,
                ValueType = _mapper.Map<DiscountValueType>(request.ValueType),
                Value = request.Value,
                IsActive = request.IsActive,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CreatedAt = DateTime.UtcNow,
                DiscountProducts = discountType == DiscountType.ProductSpecific
                    ? request.ProductIds.Select(id => new DiscountProduct { ProductId = id }).ToList()
                    : [],
                DiscountCollections = discountType == DiscountType.Collection
                    ? request.CollectionIds.Select(id => new DiscountCollection { CollectionId = id }).ToList()
                    : [],
                BundleProducts = discountType == DiscountType.Bundle
                    ? request.BundleProductIds.Select(id => new DiscountBundleProduct { ProductId = id }).ToList()
                    : []
            };

            var added = await _addDiscountService.AddAsync(discount);
            return added != null ? _mapper.Map<DiscountDto>(added) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during adding discount");
            return null;
        }
    }
}
