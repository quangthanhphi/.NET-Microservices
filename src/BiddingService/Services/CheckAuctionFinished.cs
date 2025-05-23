using System;
using BiddingService.Models;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace BiddingService.Services;

public class CheckAuctionFinished : BackgroundService
{
    private readonly ILogger<CheckAuctionFinished> _logger;
    private readonly IServiceProvider _serviceProvider;

    public CheckAuctionFinished(ILogger<CheckAuctionFinished> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CheckAuctionFinished is starting.");

        stoppingToken.Register(() => _logger.LogInformation("==> Auction check is stopping."));

        while (!stoppingToken.IsCancellationRequested)
        {
          await CheckAuction(stoppingToken);

          await Task.Delay(5000, stoppingToken);
        }
    }

    private async Task CheckAuction(CancellationToken stoppingToken)
    {
        var finishedAuctions = await DB.Find<Auction>()
          .Match(x => x.AuctionEnd <= DateTime.UtcNow)
          .Match(x => !x.Finished)
          .ExecuteAsync(stoppingToken);
        
        if (finishedAuctions.Count == 0) return;

        _logger.LogInformation("==> Found {0} finished auctions.", finishedAuctions.Count);

        using var scope = _serviceProvider.CreateScope();
        var endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        foreach (var auction in finishedAuctions)
        {
          auction.Finished = true;
          await auction.SaveAsync(null, stoppingToken);

          var winningBid = await DB.Find<Bid>()
            .Match(a => a.AuctionId == auction.ID)
            .Match(b => b.BidStatus == BidStatus.Accepted)
            .Sort(x => x.Descending(s => s.Amount))
            .ExecuteFirstAsync(stoppingToken);

          await endpoint.Publish(new AuctionFinished
          {
            ItemSold = winningBid != null,
            AuctionId = auction.ID,
            Winner = winningBid?.Bidder,
            Amount = winningBid?.Amount,
            Seller = auction.Seller,
          },stoppingToken);

        }
    }
}
