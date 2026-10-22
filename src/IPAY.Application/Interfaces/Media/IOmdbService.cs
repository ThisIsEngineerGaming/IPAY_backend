using IPAY.Application.DTOs.Media;

namespace IPAY.Application.Interfaces.Media;

public interface IOmdbService
{
    //interface to api requsests
    Task<OmdbSearchResponseDto> SearchAsync(string query, int page = 1, CancellationToken cancellationToken = default);
    Task<OmdbFilmDto> GetByImdbIdAsync(string imdbId, CancellationToken cancellationToken = default);
}
