using System.Text.Json;
using Microsoft.VisualBasic;
using MongoDB.Driver;
using MongoDB.Entities;

namespace SearchService;

public static class DbInitializer
{
    public static async Task InitDB(WebApplication app)
    {
        await DB.InitAsync("SearchDb", MongoClientSettings.FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

        await DB.Index<Item>()
            .Key(x => x.Make, KeyType.Text)
            .Key(x => x.Model, KeyType.Text)
            .Key(x => x.Color, KeyType.Text)
            .CreateAsync();

        using var scope = app.Services.CreateScope();

        var httpClient = scope.ServiceProvider.GetRequiredService<AuctionServiceHttpClient>();

        var itens = await httpClient.GetItensForSearchDb();

        Console.WriteLine(itens.Count + " returned from the auction service");

        if (itens.Count != 0) await DB.SaveAsync(itens);
    }

}
