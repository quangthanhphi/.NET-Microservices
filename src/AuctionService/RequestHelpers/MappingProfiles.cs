using System;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;

namespace AuctionService.RequestHelpers;

public class MappingProfiles : Profile
{
  public MappingProfiles()
  {
    // Tạo cấu hình ánh xạ từ Auction sang AuctionDto và bao gồm các thành viên của Item
    CreateMap<Auction, AuctionDto>().IncludeMembers(x => x.Item);
    
    // Tạo cấu hình ánh xạ từ Item sang AuctionDto
    CreateMap<Item, AuctionDto>();
    
    // Tạo cấu hình ánh xạ từ CreateAuctionDto sang Auction
    // và ánh xạ toàn bộ đối tượng CreateAuctionDto sang thuộc tính Item của Auction
    CreateMap<CreateAuctionDto, Auction>()
      .ForMember(d => d.Item, opt => opt.MapFrom(s => s));
    
    // Tạo cấu hình ánh xạ từ CreateAuctionDto sang Item
    CreateMap<CreateAuctionDto, Item>();
    CreateMap<AuctionDto, AuctionCreated>();
  }
}
