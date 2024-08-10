using System;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace SearchService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine($"-- Consuming {nameof(BidPlaced)} in {nameof(SearchService)}");

        var auction = await DB.Find<Item>().OneAsync(context.Message.Id);

        if (context.Message.BidStatus.Contains("accepted")
            && context.Message.Ammount > auction.CurrentHighBid)
        {
            auction.CurrentHighBid = context.Message.Ammount;
            await auction.SaveAsync();
        }
    }
}
