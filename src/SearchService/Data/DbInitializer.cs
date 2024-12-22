using System;
using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Services;

namespace SearchService.Data;

public class DbInitializer
{
  public static async Task InitDb(WebApplication app)
  {
    Console.WriteLine("Initializing database...");
    try
    {
      var connectionString = app.Configuration.GetConnectionString("MongoDbConnection");
      Console.WriteLine($"MongoDB Connection String: {connectionString}");

      await DB.InitAsync("SearchDb", MongoClientSettings.FromConnectionString(connectionString));

      Console.WriteLine("Database initialized.");

      await DB.Index<Item>()
        .Key(x => x.Make, KeyType.Text)
        .Key(x => x.Model, KeyType.Text)
        .Key(x => x.Color, KeyType.Text)
        .CreateAsync();

      Console.WriteLine("Indexes created.");

      var count = await DB.CountAsync<Item>();
      Console.WriteLine($"Item count: {count}");

      using var scope = app.Services.CreateScope();

      var httpClient = scope.ServiceProvider.GetRequiredService<AuctionSvcHttpClient>();

      var items = await httpClient.GetItemsForSearchDb();
      Console.WriteLine($"Retrieved {items.Count} items from AuctionService.");

      if (items.Count > 0) await DB.SaveAsync(items);
      Console.WriteLine($"Inserted {items.Count} items into MongoDB.");
    }
    catch (Exception e)
    {
      Console.WriteLine($"Error initializing database: {e.Message}");
      throw;
    }
  }
}