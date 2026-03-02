using AutoMapper;
using Katino.Application.DTOs.FinanceCategory;
using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Repositories.FinanceCategoryRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.FinanceCategoryN.AddFinanceCategory;

public class AddFinanceCategoryCommandHandler : IRequestHandler<AddFinanceCategoryCommand, FinanceCategoryDto>
{
    private readonly IFinanceCategoryRepository _repo;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public AddFinanceCategoryCommandHandler(
        IFinanceCategoryRepository repo,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _repo = repo;
        _logger = loggerFactory?.CreateLogger(nameof(AddFinanceCategoryCommandHandler));
        _mapper = mapper;
    }

    public async Task<FinanceCategoryDto> Handle(AddFinanceCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling add finance category request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var name = (request.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Category name is required.");
            }

            var categoryType = _mapper.Map<FinanceCategoryType>(request.FinanceCategoryType);
            if (await _repo.ExistsActiveByTypeAndNameAsync(categoryType, name))
            {
                throw new ArgumentException($"Category '{name}' already exists.");
            }

            var existing = await _repo.GetByTypeAndNameAsync(categoryType, name);
            if (existing != null)
            {
                existing.IsActive = true;
                await _repo.Update(existing);
                await _repo.Save();
                var existingDto = _mapper.Map<FinanceCategoryDto>(existing);
                existingDto.IsActive = true;

                return existingDto;
            }

            var entity = new FinanceCategory
            {
                Id = Guid.NewGuid(),
                Type = categoryType,
                Name = name,
                IsActive = true,
                SortOrder = 0,
            };

            var inserted = await _repo.Insert(entity);
            await _repo.Save();

            return _mapper.Map<FinanceCategoryDto>(inserted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during adding finance category");
            return null;
        }
    }
}
