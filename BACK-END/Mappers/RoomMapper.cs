using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BACK_END.DTOs.RoomDto;
using BACK_END.Models;

namespace BACK_END.Mappers
{
    public static class RoomMapper
    {
        public static GetAllRoomRepository MapToGetAllRoomRepository(this Room model)
        {
            return new GetAllRoomRepository
            {
                Id = model.Id,
                Area = model.Area,
                Price = model.Price,
                Quantity = model.Quantity,
                Description = model.Description,
                RoomTypeName = model.RoomType?.Name ?? "",
                BoardingHouse = new DTOs.RoomDto.BoardingHouse {
                    Id = model.BoardingHouse?.Id ?? 0,
                    NameOwner = model.BoardingHouse?.User?.FullName ?? "",
                    Address = model.BoardingHouse?.Address ?? "",
                }
            };
        }
    }
}