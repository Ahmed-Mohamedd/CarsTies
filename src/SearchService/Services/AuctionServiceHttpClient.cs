using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services
{
    public class AuctionServiceHttpClient(IConfiguration config , HttpClient httpClient)
    {
        public async Task<List<Item>> GetItemsForSearchDb(WebApplication app)
        {
            string lastUpdated = DateTime.MinValue.ToString();
            var scope = app.Services.CreateScope();
            var distributedCache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();

            // gets the last updated item from the cache.
             var itemsfromCache = await distributedCache.GetStringAsync("auctions");
            var itemss = itemsfromCache switch
            {
                null => new List<Item>(),
                _ => JsonSerializer.Deserialize<List<Item>>(itemsfromCache)
            };

            if (itemss.Count > 0)
            {
                 lastUpdated = itemss.OrderByDescending(x => x.UpdatedAt).FirstOrDefault().UpdatedAt.ToString();
            }

            //var lastUpdated = await DB.Find<Item, string>()
            //    .Sort(x => x.Descending(x => x.UpdatedAt))
            //    .Project(x => x.UpdatedAt.ToString())
            //    .ExecuteFirstAsync();

            var auctionURL = config["AuctionServiceUrl"]
                ?? throw new ArgumentNullException("Cannot get auction address");

            var url = auctionURL + "/api/auctions";

            if (!string.IsNullOrEmpty(lastUpdated))
            {
                url += $"?date={lastUpdated}";
            }

            var items = await httpClient.GetFromJsonAsync<List<Item>>(url);

            return items ?? [];
        }
    }
}
