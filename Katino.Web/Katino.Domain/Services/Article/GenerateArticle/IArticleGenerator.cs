namespace Katino.Domain.Services.Article.GenerateArticle;

public interface IArticleGenerator
{
    Task<string> GenerateUniqueArticleAsync();
}
