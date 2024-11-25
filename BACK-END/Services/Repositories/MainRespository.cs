﻿using AutoMapper;
using BACK_END.Data;
using BACK_END.DTOs.MainDto;
using BACK_END.DTOs.MotelDto;
using BACK_END.Services.Interfaces;
using BACK_END.Services.Paging;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace BACK_END.Services.Repositories
{
    public class MainRespository : IGetTro
    {
        private readonly BACK_ENDContext _db;
        private readonly FirebaseStorageService _firebaseStorageService;
        private readonly IMapper _mapper;
        public MainRespository(BACK_ENDContext db, FirebaseStorageService firebaseStorageService, IMapper mapper)
        {
            _db = db;
            _firebaseStorageService = firebaseStorageService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RoomTypeWithPackageDTO>> GetRoomTypesWithFeature()
        {
            var roomTypes = await _db.Room_Type
                .Include(r => r.Motel)
                .Include(r => r.Images)
                .ToListAsync();

            var result = roomTypes.Select(roomType => new RoomTypeWithPackageDTO
            {
                Id = roomType.Id,
                Price = roomType.Price,
                Name = roomType.Name,
                Address = roomType.Motel?.Address,
                Images = roomType.Images?.Select(i => new ImageDTO
                {
                    Id = i.Id,
                    Link = i.Link,
                    Type = i.Type
                }).ToList(),
                IsFeatured = _db.Package_User.Any(pu => pu.UserId == roomType.Motel.UserId)
            }).Where(r => r.IsFeatured)
              .ToList();

            return result;
        }
        public async Task<List<RoomTypeWithPackageDTO>> GetNewRoomTypesAsync()
        {
            var recentDate = DateTime.Now.AddMonths(-3);
            var roomTypes = await _db.Room_Type
                .Include(rt => rt.Motel)
                .Include(rt => rt.Images)
                .Where(rt => rt.Motel.CreateDate >= recentDate).OrderByDescending(rt => rt.Motel.CreateDate)
                .Select(rt => new RoomTypeWithPackageDTO
                {
                    Id = rt.Id,
                    Name = rt.Name,
                    Price = rt.Price,
                    Address = rt.Motel.Address,
                    Images = rt.Images.Select(i => new ImageDTO
                    {
                        Id = i.Id,
                        Link = i.Link,
                        Type = i.Type
                    }).ToList(),
                })
                .ToListAsync();

            return roomTypes;
        }

        public async Task<List<RoomTypeWithPackageDTO>> GetRoomTypesUnderOneMillionAsync()
        {
            var roomTypes = await _db.Room_Type
        .Include(rt => rt.Motel)
        .Include(rt => rt.Images)
        .Where(rt => rt.Price < 1000000)
        .OrderByDescending(rt => rt.Motel.CreateDate)
        .Select(rt => new RoomTypeWithPackageDTO
        {
            Id = rt.Id,
            Name = rt.Name,
            Price = rt.Price,
            Address = rt.Motel.Address,
            Images = rt.Images.Select(i => new ImageDTO
            {
                Id = i.Id,
                Link = i.Link,
                Type = i.Type
            }).ToList(),
        })
        .ToListAsync();

            return roomTypes;
        }
        public Task<RoomTypeDTO> GetRoomTypeByRoomID(int id)
        {
            //find roomtype by id
            var roomType = _db.Room_Type
                .Include(rt => rt.Motel).ThenInclude(m => m.User)
                .Include(rt => rt.Images)
                .FirstOrDefault(rt => rt.Id == id);

            if (roomType == null)
            {
                return Task.FromResult<RoomTypeDTO>(null);
            }
            //gộp mô tả motel + roomtype
            var Description = roomType.Motel.Description + ". " + roomType.Description;

            var roomTypeDTO = new RoomTypeDTO
            {
                Id = roomType.Id,
                Name = roomType.Name,
                Area = roomType.Area,
                Price = roomType.Price,
                Address = roomType.Motel.Address,
                Images = roomType.Images.Select(i => new ImageDTO
                {
                    Id = i.Id,
                    Link = i.Link,
                    Type = i.Type
                }).ToList(),
                Description = Description,
                Location = roomType.Motel.Location,
                Status = roomType.Motel.Status,
                CreateDate = roomType.CreateDate,
                UpdateDate = roomType.UpdateDate,
                FullName = roomType.Motel.User.FullName,
                PhoneNumber = roomType.Motel.User.Phone,
                Email = roomType.Motel.User.Email,
                //đếm theo số lượng roomtype của motel
                RoomTypeCount = _db.Room_Type.Count(rt => rt.MotelId == roomType.MotelId)
            };

            return Task.FromResult(roomTypeDTO);
        }

        private string? HandlerToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                return jwtToken.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token error: {ex.Message}");
                return null;
            }
        }

        public async Task<PaginatedResponse<Rentalroomuser>> GetRentalRoomHistoryAsync(string token, int pageIndex, int pageSize)
        {
            var email = HandlerToken(token);
            if (string.IsNullOrEmpty(email))
                throw new UnauthorizedAccessException("Invalid token");

            var user = await _db.User.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            // Truy vấn Room_History theo UserId
            var query = _db.Room_History
                .Where(rh => rh.UserId == user.Id)
                .Include(rh => rh.Room)
                    .ThenInclude(r => r.Room_Type)
                    .ThenInclude(rt => rt.Motel)
                .Select(rh => new Rentalroomuser
                {
                    RoomId = rh.RoomId ?? 0,
                    CreateDate = rh.CreateDate,
                    EndDate = rh.EndDate,
                    RoomNumber = rh.Room.RoomNumber,
                    Price = rh.Room.Room_Type.Price,
                    MotelName = rh.Room.Room_Type.Motel.Name
                });

            // Phân trang
            var totalItems = await query.CountAsync();
            var data = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResponse<Rentalroomuser>
            {
                TotalItems = totalItems,
                Items = data
            };
        }

        //public Task<PaginatedResponse<BillDto>> GetBillAsync(int id, int pageIndex, int pageSize)
        //{
        //    var Billdetail = _db.Room.Where
        //}
    }
}
