using IPAY.Domain.Entities.Media;

namespace IPAY.Domain.Interfaces.ForRepos.Media
{
    //episode repo, which includes basic repository<T>
    public interface IEpisodeRepo : IRepository<Episode>
    {
        /// Episodes of one series, queried in Firestore (not filtered in memory), ordered by id.
        Task<IReadOnlyList<Episode>> GetBySeriesIdAsync(int seriesId);
    }
}
