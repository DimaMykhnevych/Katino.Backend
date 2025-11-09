using Katino.Domain.Models.NovaPost;

namespace Katino.Domain.Services.NovaPost.InternetDocument;

public interface IInternetDocumentService
{
    Task<NpApiResponse<NpInternetDocumentCreationResponse>> CreateInternetDocumentAsync(CreateNovaPostInternetDocument request);
}
