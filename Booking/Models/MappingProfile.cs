using AutoMapper;
using Booking.Areas.Accounts.Models.Account;
using Booking.Areas.Admin.Models.Hotel;
using Booking.Areas.Admin.Models.Tour;
using Booking.EF;

namespace Booking.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<RegisterDTO, User>(); // Map data của RegisterDTO sang User
            CreateMap<HotelDTO, Booking.EF.Hotel>();
            CreateMap<Booking.EF.Hotel, HotelDTO>();
            //CreateMap<Hotel, HotelDTO>()
            //    .ForMember(ht => ht.Images, opt => opt.Ignore()); // Không map images từ hotel sang hotelDTO

            CreateMap<Room, RoomDTO>();
            CreateMap<RoomDTO, Room>();

            CreateMap<Booking.EF.Tour, TourDTO>();
            CreateMap<TourDTO, Booking.EF.Tour>();
        }
    }
}
