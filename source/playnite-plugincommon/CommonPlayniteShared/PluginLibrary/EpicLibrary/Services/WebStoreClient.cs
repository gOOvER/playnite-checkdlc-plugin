using CommonPlayniteShared.PluginLibrary.EpicLibrary.Models;//using EpicLibrary.Models;
using CommonPluginsShared;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CommonPlayniteShared.PluginLibrary.EpicLibrary.Services
{
    public class WebStoreClient : IDisposable
    {
        private HttpClient httpClient = new HttpClient();

        public const string GraphQLEndpoint = @"https://graphql.epicgames.com/graphql";
        public const string ProductUrlBase = @"https://store-content.ak.epicgames.com/api/{1}/content/products/{0}";//public const string ProductUrlBase = @"https://store-content.ak.epicgames.com/api/en-US/content/products/{0}";

        public WebStoreClient()
        {
        }

        public void Dispose()
        {
            httpClient.Dispose();
        }

        public async Task<List<WebStoreModels.QuerySearchResponse.Data.CatalogItem.SearchStore.SearchStoreElement>> QuerySearch(string searchTerm)
        {
            try
            {
                var query = new WebStoreModels.QuerySearch();
                query.variables.keywords = HttpUtility.UrlEncode(searchTerm);
                var content = new StringContent(Serialization.ToJson(query), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(GraphQLEndpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }
                var str = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(str) || !str.TrimStart().StartsWith("{"))
                {
                    return null;
                }
                var data = Serialization.FromJson<WebStoreModels.QuerySearchResponse>(str);
                return data?.data?.Catalog?.searchStore?.elements;
            }
            catch (Exception ex)
            {
                Playnite.SDK.LogManager.GetLogger().Error(ex, "Epic QuerySearch failed");
                return null;
            }
        }

        public async Task<WebStoreModels.ProductResponse> GetProductInfo(string productSlug, string PlayniteLanguage = "en-US")
        {
            string EpicLangCountry = CodeLang.GetEpicLangCountry(PlayniteLanguage);
            if (PlayniteLanguage == "es_ES" || PlayniteLanguage == "zh_TW")
            {
                EpicLangCountry = CodeLang.GetEpicLang(PlayniteLanguage);
            }

            var slugUri = productSlug.Split('/').First();
            var productUrl = string.Format(ProductUrlBase, slugUri, EpicLangCountry);
            var str = await httpClient.GetStringAsync(productUrl);
            return Serialization.FromJson<WebStoreModels.ProductResponse>(str);
        }
    }
}
