using AutoMapper;
using Contracts;
using MassTransit;
using SearchService.Models;
using SearchService.Services;

namespace SearchService.Consumers
{
    public class AuctionCreatedConsumer(IMapper _mapper , ILogger<AuctionCreatedConsumer>_logger , RedisService _redisService) : IConsumer<AuctionCreated>
    {
        public async Task Consume(ConsumeContext<AuctionCreated> context)
        {
            _logger.LogInformation($"--> Consuming Auction Created:  ${context.Message.Id}");
            var items = await _redisService.GetCachedDataAsync();
            items.Add(_mapper.Map<Item>(context.Message));
            await _redisService.SetCachedDataAsync(items);
        }
    }
}
