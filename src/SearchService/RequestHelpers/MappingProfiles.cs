using AutoMapper;
using Contracts;
using SearchService.Models;

namespace SearchService;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<AuctionCreated, Item>();
        CreateMap<AuctionDeleted, DeleteItem>();
        CreateMap<AuctionUpdated, UpdateItem>();

        CreateMap<Item, UpdateItem>().ReverseMap();
        CreateMap<Item, DeleteItem>().ReverseMap();
    }
}
