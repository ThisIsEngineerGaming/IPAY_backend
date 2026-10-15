using IPAY.Domain.Entities.Media;
using IPAY.Domain.Interfaces.ForRepos.Media;
using IPAY.Infrastructure.Persistence;
using IPAY.Infrastructure.Persistence.Documents;
using Google.Cloud.Firestore;

namespace IPAY.Infrastructure.Repositories.Media
{
    public class EpisodeRepo(FirestoreDb database, string collectionName)
        : FirestoreRepository<Episode, EpisodeDocument>(
            database, collectionName, EpisodeDocument.FromEntity, document => document.ToEntity()),
          IEpisodeRepo
    {
        private readonly CollectionReference _episodes = database.Collection(collectionName);

        public async Task<IReadOnlyList<Episode>> GetBySeriesIdAsync(int seriesId)
        {
            var snapshot = await _episodes.WhereEqualTo("SerialId", seriesId).GetSnapshotAsync();
            // Document ids are strings ("1", "10", "2"); order numerically here. A series has few episodes.
            return snapshot.Documents
                .Select(document => document.ConvertTo<EpisodeDocument>().ToEntity())
                .OrderBy(episode => episode.Id)
                .ToList();
        }
    }
}
