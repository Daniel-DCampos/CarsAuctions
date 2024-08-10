using System;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    private readonly AuctionDbContext _auctionDb;

    public BidPlacedConsumer(AuctionDbContext auctionDb)
    {
        _auctionDb = auctionDb;
    }
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine($"---- Consuming {nameof(BidPlaced)}");

        var auction = await _auctionDb.Auctions.FindAsync(context.Message.AuctionId);

        if (auction == null)
            return;

        if (auction.CurrentHighBid == null
            || context.Message.BidStatus.Contains("accepted")
            && context.Message.Ammount > auction.CurrentHighBid)
        {
            auction.CurrentHighBid = context.Message.Ammount;
            await _auctionDb.SaveChangesAsync();
        }
    }
}
