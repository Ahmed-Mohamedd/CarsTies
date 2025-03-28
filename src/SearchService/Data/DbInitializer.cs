﻿using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Services;

namespace SearchService.Data
{
    public static class DbInitializer
    {
        public static async Task InitDb(WebApplication app)
        {
            await DB.InitAsync("SearchDb",
                MongoClientSettings.FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

            await DB.Index<Item>()
                .Key(x => x.Make, KeyType.Text)
                .Key(x => x.Model, KeyType.Text)
                .Key(x => x.Color, KeyType.Text)
                .CreateAsync();

            //var count = await DB.CountAsync<Item>();
            //if(count == 0)
            //{
            //    Console.WriteLine(" no data - wil attempt to send");
            //    var itemData = await File.ReadAllTextAsync("Data/auctions.json");
            //    var jsonDeSeralizeditems =  JsonSerializer.Deserialize<List<Item>>(itemData, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            //    await DB.SaveAsync(jsonDeSeralizeditems);
            //}

            using var scope = app.Services.CreateScope();

            var httpClient = scope.ServiceProvider.GetRequiredService<AuctionServiceHttpClient>();

            var items = await httpClient.GetItemsForSearchDb();

            Console.WriteLine(items.Count + " returned from auction service");

            if (items.Count > 0) await DB.SaveAsync(items);
        }
    }
}
