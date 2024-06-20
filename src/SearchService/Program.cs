using System.Net;
using System.Runtime.Intrinsics.Arm;
using MongoDB.Driver;
using MongoDB.Entities;
using Polly;
using Polly.Extensions.Http;
using SearchService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddHttpClient<AuctionServiceHttpClient>().AddPolicyHandler(GetAsyncPolicy());

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(async () =>
{
    await DbInitializer.InitDB(app);
});

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetAsyncPolicy()
    => HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));