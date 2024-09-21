using static QuickBookResponseModel;

namespace CanamDistributors.Interfaces
{
    public interface IQuickBookService
    {
        string InitiateAuthQuickBook();
        Task<TokenResponse> GetAuthTokensAsync(string code);
        Task UpdateInventoryItemsAsync(string accessToken, string realmId);
    }
}
