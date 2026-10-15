using IPAY.Domain.Entities.Media;

namespace IPAY.Domain.Interfaces.ForRepos.Media
{
    /// Series persistence: generic CRUD plus paging, filtering and sorting (same rules as IFilmRepo)
    public interface ISeriesRepo : IRepository<Series>
    {
        Task<IReadOnlyList<Series>> GetPageAsync(
            int limit, string? lastDocId, int? genreId, string? search, string? sortBy, string? sortDir);
    }
}
