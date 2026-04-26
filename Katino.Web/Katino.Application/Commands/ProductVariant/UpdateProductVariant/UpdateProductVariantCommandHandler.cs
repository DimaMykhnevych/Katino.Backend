using AutoMapper;
using Katino.Domain.Entities;
using Katino.Domain.Services.ProductVariantN.UpdateProductVariantService;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.ProductVariantN.UpdateProductVariant;

public class UpdateProductVariantCommandHandler : IRequestHandler<UpdateProductVariantCommand, bool>
{
    private readonly IUpdateProductVariantService _updateProductVariantService;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public UpdateProductVariantCommandHandler(
        IUpdateProductVariantService updateProductVariantService,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _updateProductVariantService = updateProductVariantService;
        _logger = loggerFactory?.CreateLogger(nameof(UpdateProductVariantCommandHandler));
        _mapper = mapper;
    }

    public async Task<bool> Handle(UpdateProductVariantCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling update product variant request");
        ArgumentNullException.ThrowIfNull(request);

        ProductVariant pv = _mapper.Map<ProductVariant>(request.ProductVariant);
        return await _updateProductVariantService
            .UpdateProductVariantAsync(pv, request.ProductVariant.NewPhotos, request.ProductVariant.PhotoIdsToDelete, request.ProductVariant.SewerIds);
    }
}
