using ExamTest.Application.DTOs.Media;

namespace ExamTest.Application.Services.Media;

public interface IOmdbService
{
    Task<OmdbSearchResponseDto> SearchAsync(string query, int page = 1, CancellationToken cancellationToken = default);
    Task<OmdbFilmDto> GetByImdbIdAsync(string imdbId, CancellationToken cancellationToken = default);
}
