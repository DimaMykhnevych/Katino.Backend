using Katino.Domain.Entities;
using Katino.Domain.Models.NovaPost;

namespace Katino.Domain.Services.NovaPost.InternetDocument;

public interface IInternetDocumentService
{
    Task<NpApiResponse<NpInternetDocumentCreationResponse>> CreateInternetDocumentAsync(CreateNovaPostInternetDocument request, string existingDocRef = null);
    CreateNovaPostInternetDocument CreateNovaPostInternetDocument(Order orderWithAllInfo);
    UpdateNovaPostInternetDocument CreateUpdateNovaPostInternetDocument(Order orderWithAllInfo);
}
