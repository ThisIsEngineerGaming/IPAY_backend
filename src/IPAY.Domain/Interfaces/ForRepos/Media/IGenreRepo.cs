using IPAY.Domain.Entities.Media;

namespace IPAY.Domain.Interfaces.ForRepos.Media
{
    public interface IGenreRepo : IRepository<Genre>
    {
        // Atomic array updates (Firestore ArrayUnion / ArrayRemove): no read-modify-write,
        // so two concurrent requests can't overwrite each other. All of them are no-ops for a missing genre.
        Task AddFilmAsync(int genreId, int filmId);
        Task RemoveFilmAsync(int genreId, int filmId);
        Task AddSeriesAsync(int genreId, int seriesId);
        Task RemoveSeriesAsync(int genreId, int seriesId);
        Task ClearAsync(int genreId);
    }
}
