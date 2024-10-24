using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BACK_END.DTOs.RoomDto;
using BACK_END.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BACK_END.Mappers
{
    public static class RoomMapper
    {
        public static GetAllRoomRepositoryDto MapToGetAllRoomRepository(this Room model)
        {
            var room = new GetAllRoomRepositoryDto
            {
                Id = model.Id,
                Price = model.Price,
                Rating = (model.Motel?.Reviews?.Count > 0 ? (model.Motel.Reviews.Average(x => x.Rating)) : 0),
                TotalRating = model.Motel?.Reviews?.Count ?? 0,
                Motel = new DTOs.RoomDto.ModelDto
                {
                    NameOwner = model.Motel?.Name ?? "",
                    Address = model.Motel?.Address ?? "",
                    Location = model.Motel?.Location ?? "",
                    TotalRoom = model.Motel?.Rooms?.Count ?? 0
                }
            };

            foreach (var item in model?.Motel?.Prices)
            {
                if (item != null && item.IsActive == true)
                {
                    room.PriceOther = new DTOs.RoomDto.PriceDto
                    {
                        Electric = item?.Electric ?? 0.00,
                        Water = item?.Water ?? 0.00,
                        Other = item?.Other ?? 0.00
                    };
                }
            }
            return room;
        }
        public static GetAllMotelByAdminDto MapToGetAllMotelByAdmin(this Motel model)
        {
            return new GetAllMotelByAdminDto
            {
                Id = model.Id,
                Name = model.Name,
                Address = model.Address,
                Acreage = model.Acreage,
                CreateDate = model.CreateDate,
                NameOwner = model.User?.FullName ?? "",
                TotalRoom = model?.Rooms?.Count ?? 0,
                Status = model?.Status ?? 0
            };
        }

        public static (Motel motel, List<Room> ListRoom, Price price) MapToMotelAndRoom(this AddMotelAndRoomDto dto)
        {
            var motel = new Motel
            {
                Name = dto.Name,
                Address = dto.Address,
                Acreage = dto.Acreage,
                UserId = dto.UserId,
                Status = 1,
                CreateDate = DateTime.Now
            };

            var ListRoom = new List<Room>();
            for(int i = 0; i < dto.TotalRoom; i++)
            {
                ListRoom.Add(new Room{
                    RoomNumber = i + 1,
                    Price = dto.Price,
                    Status = 1
                });
            }

            var price = new Price
            {
                Water = dto.Price,
                Electric = dto.Price,
                Other = dto.Price,
                MotelId = motel.Id
            };

            return (motel, ListRoom, price);
        }
        public static (Motel motel, Price price) MapToMotelAndPrice(this UpdateMotelDto dto)
        {
            var motel = new Motel
            {
                Name = dto.Name,
                Address = dto.Address,
                Acreage = dto.Acreage
            };

            var price = new Price
            {
                Water = dto.Price?.Water ?? 0,
                Electric = dto.Price?.Electric ?? 0,
                Other = dto.Price?.Other ?? 0
            };

            return (motel, price);
        }
        public static UpdateMotelDto MapToUpdateMotelDto(this Motel motel)
        {
            return new UpdateMotelDto
            {
                Name = motel.Name,
                Address = motel.Address,
                Acreage = motel.Acreage,
                Price = motel.Prices.Select(x => new UpdatePriceDto
                {
                    Water = x.Water,
                    Electric = x.Electric,
                    Other = x.Other
                }).FirstOrDefault()
            };
        }
    }
}
