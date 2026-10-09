using IPAY.Application.DTOs.Media;

namespace IPAY.Application.Interfaces.Media
{
    public interface IFilmImportService
    {
        /// <summary>Fetches a film from OMDb by IMDb id and stores it. Existing genres are linked; missing ones are never created.</summary>
        Task<FilmImportResult> ImportAsync(string imdbId, CancellationToken cancellationToken = default);
    }
}
