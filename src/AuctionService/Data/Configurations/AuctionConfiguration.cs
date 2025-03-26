using AuctionService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionService.Data.Configurations
{
    public class AuctionConfiguration : IEntityTypeConfiguration<Auction>
    {
        public void Configure(EntityTypeBuilder<Auction> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasOne(a => a.Item)
                   .WithOne(i => i.Auction)
                   .HasForeignKey<Auction>(a => a.ItemId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
