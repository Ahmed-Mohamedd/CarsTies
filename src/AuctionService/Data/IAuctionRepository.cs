using AuctionService.DTOs;
using AuctionService.Models;

namespace AuctionService.Data
{
    public interface IAuctionRepository
    {
        Task<List<AuctionDto>> GetAuctionsAsync(string? date);
        Task<AuctionDto?> GetAuctionByIdAsync(Guid id);
        Task<Auction?> GetAuctionEntityById(Guid id);
        Task AddAuction(Auction auction);
        void RemoveAuction(Auction auction);
        Task<bool> SaveChangesAsync();
    }
}
