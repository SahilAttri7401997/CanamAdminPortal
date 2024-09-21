using CanamDistributors.Data;
using CanamDistributors.Entity;
using CanamDistributors.Interfaces;
using CanamDistributors.Models;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NuGet.Packaging;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using static QuickBookResponseModel;

namespace CanamDistributors.Services
{
    public class QuickBookService : IQuickBookService
    {
        private readonly AppSettings _appSettings;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<QuickBookService> _logger;
        private readonly HttpClient _httpClient;

        public QuickBookService(IOptions<AppSettings> appSettings, ApplicationDbContext context, ILogger<QuickBookService> logger)
        {
            _appSettings = appSettings.Value;
            _context = context;
            _logger = logger;
            _httpClient = new HttpClient();
        }

        public string InitiateAuthQuickBook()
        {
            var state = Guid.NewGuid().ToString("N");
            var url = $"https://appcenter.intuit.com/connect/oauth2?client_id={_appSettings.ClientId}&redirect_uri={_appSettings.RedirectUri}&response_type=code&scope={_appSettings.Scopes}&state={state}";
            return url;
        }

        public async Task<TokenResponse> GetAuthTokensAsync(string code)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "https://oauth.platform.intuit.com/oauth2/v1/tokens/bearer")
                {
                    Headers =
                    {
                        Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_appSettings.ClientId}:{_appSettings.ClientSecret}"))),
                    },
                    Content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "grant_type", "authorization_code" },
                        { "code", code },
                        { "redirect_uri", _appSettings.RedirectUri }
                    })
                };

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TokenResponse>(responseContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get authorization tokens.");
                throw;
            }
        }

        public async Task UpdateInventoryItemsAsync(string accessToken, string realmId)
        {
            try
            {
                var inventoryItems = await FetchInventoryItemsAsync(accessToken, realmId);
                var productEntities = TransformToProductEntity(inventoryItems);
                await SaveEntitiesToDatabaseAsync(productEntities, SaveProductsToDatabaseAsync);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update inventory items.");
                throw;
            }
        }
        public async Task<List<InventoryItem>> FetchInventoryItemsAsync(string accessToken, string realmId)
        {
            var inventoryItems = new List<InventoryItem>();
            var startPosition = 1;
            const int maxResultsPerPage = 100;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            while (true)
            {
                try
                {
                    var query = $"SELECT * FROM Item STARTPOSITION {startPosition} MAXRESULTS {maxResultsPerPage}";
                    var response = await _httpClient.GetAsync($"https://quickbooks.api.intuit.com/v3/company/{realmId}/query?query={Uri.EscapeDataString(query)}");

                    var contentType = response.Content.Headers.ContentType.MediaType;
                    if (contentType != "application/xml")
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Expected XML but received {contentType}. Response content: {responseContent}");
                        throw new InvalidOperationException("Unexpected content type.");
                    }

                    response.EnsureSuccessStatusCode();
                    var responseContentXml = await response.Content.ReadAsStringAsync();

                    var serializer = new XmlSerializer(typeof(IntuitResponse));
                    using (var reader = new StringReader(responseContentXml))
                    {
                        var intuitResponse = (IntuitResponse)serializer.Deserialize(reader);
                        if (intuitResponse?.QueryResponse?.Items == null || !intuitResponse.QueryResponse.Items.Any())
                            break;

                        inventoryItems.AddRange(intuitResponse.QueryResponse.Items);
                        startPosition += maxResultsPerPage;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching inventory items from QuickBooks.");
                    throw;
                }
            }

            return inventoryItems;
        }

        private List<Products> TransformToProductEntity(List<InventoryItem> items)
        {
            return items.Select(item => new Products
            {
                Id = item.Id,
                SyncToken = item.SyncToken,
                Name = item.Name,
                Description = item.Description,
                Active = item.Active,
                SubItem = item.SubItem,
                ParentRefName = item.ParentRef?.Name,
                ParentRefText = item.ParentRef?.Text,
                Level = item.Level,
                FullyQualifiedName = item.FullyQualifiedName,
                Taxable = item.Taxable,
                SalesTaxIncluded = item.SalesTaxIncluded,
                UnitPrice = item.UnitPrice != null ? Convert.ToDecimal(item.UnitPrice) : (decimal?)null,
                Type = item.Type,
                IncomeAccountRefName = item.IncomeAccountRef?.Name,
                IncomeAccountRefText = item.IncomeAccountRef?.Text,
                PurchaseTaxIncluded = item.PurchaseTaxIncluded,
                PurchaseCost = item.PurchaseCost != null ? Convert.ToDecimal(item.PurchaseCost) : (decimal?)null,
                ExpenseAccountRefName = item.ExpenseAccountRef?.Name,
                ExpenseAccountRefText = item.ExpenseAccountRef?.Text,
                AssetAccountRefName = item.AssetAccountRef?.Name,
                AssetAccountRefText = item.AssetAccountRef?.Text,
                TrackQtyOnHand = item.TrackQtyOnHand,
                QtyOnHand = item.QtyOnHand != null ? Convert.ToDecimal(item.QtyOnHand) : (decimal?)null,
                ReorderPoint = item.ReorderPoint != null ? Convert.ToDecimal(item.ReorderPoint) : (decimal?)null,
                SalesTaxCodeRefName = item.SalesTaxCodeRef?.Name,
                SalesTaxCodeRefText = item.SalesTaxCodeRef?.Text,
                InvStartDate = Convert.ToDateTime(item.InvStartDate),
                _domain = item.Domain,
                _sparse = item.Sparse,
                CreationDate = Convert.ToDateTime(item.MetaData?.CreateTime),
                LastModifiedDate = Convert.ToDateTime(item.MetaData?.LastUpdatedTime),
                IsAdminAdded = false
            }).OrderBy(p => p.Name ?? string.Empty).ToList();
        }

        private async Task SaveEntitiesToDatabaseAsync<TModel>(List<TModel> items, Func<List<TModel>, Task> saveAction)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Delete existing entities based on table name
                var tableName = typeof(TModel).Name;
                await _context.Database.ExecuteSqlRawAsync($"DELETE FROM {tableName}");

                // Save new entities
                await saveAction(items);

                // Commit transaction
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving entities to the database.");
                // Rollback on error
                await transaction.RollbackAsync();
                throw;
            }
        }
        private async Task SaveProductsToDatabaseAsync(List<Products> items) =>
            await _context.BulkInsertAsync(items);
    }
}
