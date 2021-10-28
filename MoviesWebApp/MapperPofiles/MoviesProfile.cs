using AutoMapper;
using MoviesWebApp.Models;
using MoviesWebApp.ViewModels;

namespace MoviesWebApp.MapperPofiles
{
    public class MoviesProfile : Profile
    {
        public MoviesProfile()
        {
            CreateMap<Movie, MoviewFormVM>()
                .ForMember(dest => dest.Genres, src => src.MapFrom(src => src.Genre))
                .ReverseMap();
        }
    }
}
