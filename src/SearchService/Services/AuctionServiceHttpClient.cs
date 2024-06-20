using MongoDB.Entities;

namespace SearchService;

public class AuctionServiceHttpClient
{
    private readonly HttpClient _client;
    private readonly IConfiguration _configuration;

    public AuctionServiceHttpClient(HttpClient client, IConfiguration configuration)
    {
        _client = client;
        _configuration = configuration;
    }

    public async Task<List<Item>> GetItensForSearchDb()
    {
        var lastUpdated = await DB.Find<Item, string>()
            .Sort(x => x.Descending(x => x.UpdatedAt))
            .Project(x => x.UpdatedAt.ToString())
            .ExecuteFirstAsync();

        return await _client.GetFromJsonAsync<List<Item>>(_configuration.GetSection("AuctionServiceUrl").Value + "/api/auctions?date=" + lastUpdated);
    }
}
