using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;
using SearchService.Services;

namespace SearchService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController(RedisService _redisService , ILogger<SearchController> _logger) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams )
        {
            try
            {
                if (searchParams.PageNumber < 1) searchParams.PageNumber = 1;
                if (searchParams.PageSize < 1) searchParams.PageSize = 4;

                var items = await _redisService.GetCachedDataAsync();

                // Apply filtering
                var filteredItems = items.AsQueryable();

                if (!string.IsNullOrEmpty(searchParams.Seller))
                    filteredItems = filteredItems.Where(i => i.Seller == searchParams.Seller);

                if (!string.IsNullOrEmpty(searchParams.Winner))
                    filteredItems = filteredItems.Where(i => i.Winner == searchParams.Winner);

                if (!string.IsNullOrEmpty(searchParams.OrderBy))
                {
                    filteredItems = searchParams.OrderBy switch
                    {
                       "make" => filteredItems.OrderByDescending(x =>  x.Make)
                                              .OrderBy(x => x.Model),
                        "new" => filteredItems.OrderByDescending(x => x.CreatedAt),
                       _ => filteredItems.OrderByDescending(x => x.AuctionEnd)
                    };
                }
                

                if (!string.IsNullOrEmpty(searchParams.FilterBy))
                {

                    filteredItems = searchParams.FilterBy switch
                    {
                        "finished" => filteredItems.Where(x => x.AuctionEnd < DateTime.UtcNow),
                        "endingSoon" => filteredItems.Where(x => x.AuctionEnd < DateTime.UtcNow.AddHours(6) && x.AuctionEnd > DateTime.UtcNow),
                        _ => filteredItems.Where(x => x.AuctionEnd > DateTime.UtcNow) // live
                    };
                }
                    

                

                // Total items count after filtering
                int totalCount = filteredItems.Count();

                // Apply Pagination
                var pagedItems = filteredItems
                    .Skip((searchParams.PageNumber - 1) * searchParams.PageSize)
                    .Take(searchParams.PageSize)
                    .ToList();

                // Calculate Page Count
                int pageCount = (int)Math.Ceiling((double)totalCount / searchParams.PageSize);

                return Ok(new
                {
                    Data = pagedItems,
                    TotalCount = totalCount,
                    PageCount = pageCount,
                    PageNumber = searchParams.PageNumber,
                    PageSize = searchParams.PageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing search query.");
                return StatusCode(500, "An error occurred while processing your request.");
            }

           
        }
    }
}
