using System;
using AuctionService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests.Util;

public static class ServiceCollectionExtensions
{
  public static void RemoveDbContext<T>(this IServiceCollection services)
  {
    // Tìm và xóa cấu hình DbContext mặc định nếu có
      var descriptor = services.SingleOrDefault(d =>
          d.ServiceType == typeof(DbContextOptions<AuctionDbContext>));
      if (descriptor != null) services.Remove(descriptor);
  }

  public static void EnsureCreated<T>(this IServiceCollection services)
  {
    // Tạo ServiceProvider để lấy các service đã đăng ký
      var sp = services.BuildServiceProvider();

      // Tạo scope mới để truy cập các service
      using var scope = sp.CreateScope();
      var scopedServices = scope.ServiceProvider;

      // Lấy AuctionDbContext từ service provider
      var db = scopedServices.GetRequiredService<AuctionDbContext>();

      // Chạy migrate database để đảm bảo schema được cập nhật trong test
      db.Database.Migrate();
      DbHelper.InitDbForTests(db);
  }
}
