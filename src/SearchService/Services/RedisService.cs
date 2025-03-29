using System.Text.Json;
using System.Text.Json.Serialization;
using SearchService.Models;
using StackExchange.Redis;

namespace SearchService.Services
{
    public class RedisService
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public RedisService(string connectionString)
        {
            var options = ConfigurationOptions.Parse(connectionString);
            options.ConnectRetry = 3; // Retry up to 3 times
            options.ConnectTimeout = 5000; // Timeout in 5s

            _redis = ConnectionMultiplexer.Connect(options);
            _db = _redis.GetDatabase();
        }

        public async Task<List<Item>> GetCachedDataAsync()
        {
            string? cachedData =  await _db.StringGetAsync("items");
            var options  = new JsonSerializerOptions(){ PropertyNamingPolicy = JsonNamingPolicy.CamelCase};
            return cachedData != null
                ? JsonSerializer.Deserialize<List<Item>>(cachedData,options) ?? new List<Item>()
                : new List<Item>();
        }

        public async Task SetCachedDataAsync(List<Item> items)
        {
            var options = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            string jsonData = JsonSerializer.Serialize(items , options);
            await _db.StringSetAsync("items", jsonData, TimeSpan.FromDays(30));
        }
    }

}
