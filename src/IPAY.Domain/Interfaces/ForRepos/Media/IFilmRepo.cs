using IPAY.Domain.Entities.Media;

namespace IPAY.Domain.Interfaces.ForRepos.Media
{
    /// Film persistence: generic CRUD plus paging, filtering and sorting.
    public interface IFilmRepo : IRepository<Film>
    {
        /// Only films whose GenreIds contain this id.
        /// Case-insensitive name prefix. When set, results are ordered by name and sortBy is ignored.
        /// rating | year | date (anything else = document id order).
        /// asc | desc
        Task<IReadOnlyList<Film>> GetPageAsync(
            int limit, string? lastDocId, int? genreId, string? search, string? sortBy, string? sortDir);

        Task<Film?> GetByImdbIdAsync(string imdbId);
    }
}
