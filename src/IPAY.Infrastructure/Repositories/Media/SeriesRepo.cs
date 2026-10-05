using IPAY.Domain.Entities.Media;
using IPAY.Domain.Interfaces.ForRepos.Media;
using IPAY.Infrastructure.Persistence;
using IPAY.Infrastructure.Persistence.Documents;
using Google.Cloud.Firestore;

namespace IPAY.Infrastructure.Repositories.Media
{
    public class SeriesRepo(FirestoreDb database, string collectionName)
        : FirestoreRepository<Series, SeriesDocument>(
            database, collectionName, SeriesDocument.FromEntity, document => document.ToEntity()),
          ISeriesRepo
    {
        private readonly CollectionReference _series = database.Collection(collectionName);

        public async Task<IReadOnlyList<Series>> GetPageAsync(
            int limit, string? lastDocId, int? genreId, string? search, string? sortBy, string? sortDir)
        {
            var query = await MediaPageQuery.BuildAsync(
                _series, limit, lastDocId, genreId, search, sortBy, sortDir);

            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Documents
                .Select(document => document.ConvertTo<SeriesDocument>().ToEntity())
                .ToList();
        }
    }
}
