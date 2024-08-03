using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionsController(AuctionDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDTO>>> GetAllAuctions(string date)
    {
        var query = _context.Auctions.OrderBy(a => a.Item.Make).AsQueryable();

        if (!string.IsNullOrEmpty(date))
        {
            query = query.Where(a => a.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }

        return await query.ProjectTo<AuctionDTO>(_mapper.ConfigurationProvider).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDTO>> GetAuctiondById(Guid id)
    {
        var auction = await _context.Auctions
                        .Include(a => a.Item)
                        .FirstOrDefaultAsync(a => a.Id == id);

        if (auction == null) return NotFound();

        return _mapper.Map<AuctionDTO>(auction);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<AuctionDTO>> CreateAuction(CreateAuctionDTO auctionDTO)
    {
        var auction = _mapper.Map<Auction>(auctionDTO);

        auction.Seller = User.Identity.Name;

        await _context.Auctions.AddAsync(auction);

        var newAuction = _mapper.Map<AuctionDTO>(auction);

        await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

        var result = await _context.SaveChangesAsync() > 0;

        if (!result) return BadRequest("Could not save changes to DB");

        return CreatedAtAction(nameof(GetAuctiondById), new { auction.Id }, newAuction);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDTO auctionDTO)
    {
        var auction = await _context.Auctions
                        .Include(a => a.Item)
                        .FirstOrDefaultAsync(a => a.Id == id);

        if (auction == null)
            return NotFound();

        if (auction.Seller != User.Identity.Name)
            return Forbid();

        auction.Item.Make = auctionDTO.Make ?? auction.Item.Make;
        auction.Item.Model = auctionDTO.Model ?? auction.Item.Model;
        auction.Item.Color = auctionDTO.Color ?? auction.Item.Color;
        auction.Item.Mileage = auctionDTO.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = auctionDTO.Year ?? auction.Item.Year;

        var auctionUpdated = _mapper.Map<AuctionUpdated>(auction);

        await _publishEndpoint.Publish(auctionUpdated);

        var result = await _context.SaveChangesAsync() > 0;

        if (!result) return BadRequest("Problem saving changes to DB");

        return Ok();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions
                            .FindAsync(id);

        if (auction == null) 
            return NotFound();

         if (auction.Seller != User.Identity.Name)
            return Forbid();

        await _publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });

        _context.Auctions.Remove(auction);

        var result = await _context.SaveChangesAsync() > 0;

        if (!result) return BadRequest("Problem on delete auction on DB");

        return Ok();
    }
}
