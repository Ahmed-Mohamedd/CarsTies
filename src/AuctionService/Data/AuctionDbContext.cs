using AuctionService.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AuctionService.Data
{
    public class AuctionDbContext:DbContext
    {
        public AuctionDbContext(DbContextOptions<AuctionDbContext> options):base(options)
        {
            
        }
        public DbSet<Auction> Auctions { get; set; }
        public DbSet<Item> Items { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}
