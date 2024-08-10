using System;
using AuctionService.Enums;
using AutoMapper;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
{
    private readonly AuctionDbContext _auctionDb;

    public AuctionFinishedConsumer(AuctionDbContext auctionDb)
    {
        _auctionDb = auctionDb;
    }
    public async Task Consume(ConsumeContext<AuctionFinished> context)
    {
        Console.WriteLine($"---- Consuming {nameof(AuctionFinished)}");

        var auctionFinished = await _auctionDb.Auctions.FindAsync(context.Message.AuctionId);

        if (auctionFinished == null)
            return;

        if (context.Message.ItemSold)
        {
            auctionFinished.Winner = context.Message.Winner;
            auctionFinished.SoldAmount = context.Message.Ammount;
        }

        auctionFinished.Status = auctionFinished.SoldAmount > auctionFinished.ReservePrice ? Status.Finished : Status.ReserveNotMet;
        
        await _auctionDb.SaveChangesAsync();
    }
}
