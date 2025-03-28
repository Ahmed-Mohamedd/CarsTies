﻿using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Services;

namespace SearchService.Data
{
    public static class DbInitializer
    {
        public static async Task InitDb(WebApplication app )
        {
            using var scope = app.Services.CreateScope();
            var _redisService = scope.ServiceProvider.GetRequiredService<RedisService>();
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var httpClient = scope.ServiceProvider.GetRequiredService<AuctionServiceHttpClient>();


            // get auctions from cache if available
            var cachedAuctions = await _redisService.GetCachedDataAsync();
            // If the cache is empty, we need to consume message published by the auction service
            if (cachedAuctions == null || cachedAuctions.Count == 0)
            {
               
            }


            #region MyRegion
            //await DB.InitAsync("SearchDb",
            //    MongoClientSettings.FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

            //await DB.Index<Item>()
            //    .Key(x => x.Make, KeyType.Text)
            //    .Key(x => x.Model, KeyType.Text)
            //    .Key(x => x.Color, KeyType.Text)
            //    .CreateAsync();

            //var count = await DB.CountAsync<Item>();
            //if(count == 0)
            //{
            //    Console.WriteLine(" no data - wil attempt to send");
            //    var itemData = await File.ReadAllTextAsync("Data/auctions.json");
            //    var jsonDeSeralizeditems =  JsonSerializer.Deserialize<List<Item>>(itemData, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            //    await DB.SaveAsync(jsonDeSeralizeditems);
            //}
            #endregion



            var items = await httpClient.GetItemsForSearchDb(app);
            var logger = loggerFactory.CreateLogger("logger");

            if (items.Count>0)
            {
                logger.LogInformation("Items received from auction service");
                await _redisService.SetCachedDataAsync( items);
            }

        }
    }
}
