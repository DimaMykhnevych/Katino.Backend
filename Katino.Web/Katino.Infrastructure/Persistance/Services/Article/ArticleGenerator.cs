using Katino.Domain.Services.Article.GenerateArticle;
using Katino.Infrastructure.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Katino.Infrastructure.Persistance.Services.Article;

public class ArticleGenerator : IArticleGenerator
{
    private readonly KatinoDbContext _context;

    public ArticleGenerator(KatinoDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// P-XXXXX (e.g. P-A1B2C)
    /// </summary>
    public async Task<string> GenerateUniqueArticleAsync()
    {
        const int maxAttempts = 10;
        var random = new Random();

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            string article = GenerateArticle(random);

            bool exists = await _context.ProductVariants
                .AnyAsync(pv => pv.Article == article);

            if (!exists)
            {
                return article;
            }
        }

        return $"P-{DateTime.UtcNow.Ticks % 100000:X5}";
    }

    private static string GenerateArticle(Random random)
    {
        string randomPart = GenerateRandomAlphanumeric(random, 5);
        return $"P-{randomPart}";
    }

    private static string GenerateRandomAlphanumeric(Random random, int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Range(0, length)
            .Select(_ => chars[random.Next(chars.Length)])
            .ToArray());
    }
}
