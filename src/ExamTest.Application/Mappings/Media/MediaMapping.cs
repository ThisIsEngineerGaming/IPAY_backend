using AutoMapper;
using ExamTest.Application.DTOs.Media;
using ExamTest.Domain.Entities.Media;

namespace ExamTest.Application.Mappings.Media
{
    public class MediaMapping : Profile
    {
        public MediaMapping()
        {
            // Entity → response DTO
            CreateMap<Series, SeriesDto>();
            CreateMap<Episode, EpisodeDto>();
            CreateMap<Genre, GenreDto>();
            CreateMap<Film, FilmDto>();

            // Request DTO → Entity (Id is assigned by the repository / route, never by the client)
            CreateMap<SaveSeriesDto, Series>();
            CreateMap<SaveEpisodeDto, Episode>();
            CreateMap<SaveGenreDto, Genre>();
            CreateMap<SaveFilmDto, Film>();
        }
    }
}
