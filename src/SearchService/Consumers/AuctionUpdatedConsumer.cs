using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService
{
    public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
    {
        private readonly IMapper _mapper;

        public AuctionUpdatedConsumer(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task Consume(ConsumeContext<AuctionUpdated> context)
        {
            Console.WriteLine("--> Consuming Auction updated: " + context.Message.Id);

            var itemUpdate =  _mapper.Map<UpdateItem>(context.Message);

            await DB.Update<Item>()
            .MatchID(itemUpdate.Id)
            .ModifyOnly(i => new { i.Make, i.Model, i.Year, i.Color, i.Mileage }, _mapper.Map<Item>(itemUpdate))
            .ExecuteAsync();
        }
    }
}