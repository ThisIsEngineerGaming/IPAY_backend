using IPAY.Domain.Entities.Media;

namespace IPAY.Domain.Interfaces.ForRepos.Media
{
    /// <summary>Film persistence: generic CRUD plus paging, filtering and sorting.</summary>
    public interface IFilmRepo : IRepository<Film>
    {
        /// <param name="genreId">Only films whose GenreIds contain this id.</param>
        /// <param name="search">Case-insensitive name prefix. When set, results are ordered by name and sortBy is ignored.</param>
        /// <param name="sortBy">rating | year | date (anything else = document id order).</param>
        /// <param name="sortDir">asc | desc</param>
        Task<IReadOnlyList<Film>> GetPageAsync(
            int limit, string? lastDocId, int? genreId, string? search, string? sortBy, string? sortDir);

        Task<Film?> GetByImdbIdAsync(string imdbId);
    }
}
