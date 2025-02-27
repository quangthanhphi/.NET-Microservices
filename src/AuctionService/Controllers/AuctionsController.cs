using System;
using AuctionService.Data;
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
  private readonly IAuctionRepository _repo;
  private readonly IMapper _mapper;
  private readonly IPublishEndpoint _publishEndpoint;

  public AuctionsController(IAuctionRepository repo, IMapper mapper,
    IPublishEndpoint publishEndpoint)
  {
    _repo = repo;
    _mapper = mapper;
    _publishEndpoint = publishEndpoint;// MassTransit để publish event
  }

  // Lấy danh sách tất cả các phiên đấu giá (có thể lọc theo ngày cập nhật)
  [HttpGet]
  public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
  {
      return await _repo.GetAuctionsAsync(date);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
  {
    //Truy vấn auction từ database nhưng chỉ lấy những trường cần thiết để hiển thị.Bảo mật tốt hơn: Ẩn các trường nhạy cảm như Seller, CreatedAt
    var auction = await _repo.GetAuctionByIdAsync(id);

    if (auction == null) return NotFound();

    return auction;
  }

  [Authorize]
  [HttpPost]
  public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto createAuctionDto)
  {
    var auction = _mapper.Map<Auction>(createAuctionDto); // Ánh xạ từ DTO sang entity

    auction.Seller = User.Identity.Name; // Gán seller từ thông tin user đăng nhập

    _repo.AddAuction(auction);

    var newAuction = _mapper.Map<AuctionDto>(auction); // Ánh xạ lại để trả về DTO

    await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

    var result = await _repo.SaveChangesAsync();

    if (!result) return BadRequest("Failed to create auction");

    return CreatedAtAction(nameof(GetAuctionById), // Trả về dữ liệu vừa tạo (201 Created)
      new { auction.Id }, newAuction);
  }

  [Authorize]
  [HttpPut("{id}")]
  public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
  {
    //Lấy toàn bộ entity Auction từ database để có thể cập nhật hoặc xóa.
    var auction = await _repo.GetAuctionEntityById(id);

    if (auction == null) return NotFound();

    if (auction.Seller != User.Identity.Name) return Forbid(); // Không cho phép nếu không phải seller

    // Chỉ cập nhật những giá trị không null
    auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
    auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
    auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
    auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
    auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

    // Publish sự kiện AuctionUpdated
    await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));

    var result = await _repo.SaveChangesAsync();

    if (result) return Ok();

    return BadRequest("Failed to update auction");
  }

  [Authorize]
  [HttpDelete("{id}")]
  public async Task<ActionResult> DeleteAuction(Guid id)
  {
    //Lấy toàn bộ entity Auction từ database để có thể cập nhật hoặc xóa.
    var auction = await _repo.GetAuctionEntityById(id);

    if (auction == null) return NotFound();

    if (auction.Seller != User.Identity.Name) return Forbid();

    _repo.RemoveAuction(auction);

    await _publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });

    var result = await _repo.SaveChangesAsync();

    if (!result) return BadRequest("Failed to delete auction");

    return Ok();
  }
}
