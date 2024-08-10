using System;

namespace Contracts;

public class BidPlaced
{
    public string Id { get; set; }
    public string AuctionId { get; set; }
    public string Bidder { get; set; }
    public int Ammount { get; set; }
    public string BidStatus { get; set; }
    public DateTime BidTime { get; set; }
}
