using Katino.Domain.Entities;
using Katino.Domain.Models.NovaPost;
using Newtonsoft.Json.Linq;

namespace Katino.Domain.Services.NovaPost.InternetDocument;

public interface IInternetDocumentService
{
    Task<NpApiResponse<NpInternetDocumentCreationResponse>> CreateInternetDocumentAsync(CreateNovaPostInternetDocument request, string existingDocRef = null);
    Task<bool> DeleteInternetDocumentAsync(string existingDocRef);
    Task<JArray> GetIntDocStatuses(List<string> documentNumbers);
    CreateNovaPostInternetDocument CreateNovaPostInternetDocument(Order orderWithAllInfo);
    UpdateNovaPostInternetDocument CreateUpdateNovaPostInternetDocument(Order orderWithAllInfo);
    Task<ScanSheetCreationResponse> CreateScanSheet(List<string> documentRefs, string description);
}
