using Katino.Application.Queries.CategoryN.GetCategories;
using Katino.Domain.Services.Article.GenerateArticle;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.Article.GetGeneratedArticle;

public class GetGeneratedArticleQueryHandler : IRequestHandler<GetGeneratedArticleQuery, string>
{
    private readonly ILogger _logger;
    private readonly IArticleGenerator _articleGenerator;

    public GetGeneratedArticleQueryHandler(ILoggerFactory loggerFactory, IArticleGenerator articleGenerator)
    {
        _logger = loggerFactory?.CreateLogger(nameof(GetCategoriesQueryHandler));
        _articleGenerator = articleGenerator;
    }

    public async Task<string> Handle(GetGeneratedArticleQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get generated article");
        ArgumentNullException.ThrowIfNull(request);

        return await _articleGenerator.GenerateUniqueArticleAsync();
    }
}
