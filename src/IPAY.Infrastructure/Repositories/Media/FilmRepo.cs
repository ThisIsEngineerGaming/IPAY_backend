using IPAY.Domain.Entities.Media;
using IPAY.Domain.Interfaces.ForRepos.Media;
using IPAY.Infrastructure.Persistence;
using IPAY.Infrastructure.Persistence.Documents;
using Google.Cloud.Firestore;

namespace IPAY.Infrastructure.Repositories.Media
{
    public class FilmRepo(FirestoreDb database, string collectionName)
        : FirestoreRepository<Film, FilmDocument>(
            database, collectionName, FilmDocument.FromEntity, document => document.ToEntity()),
          IFilmRepo
    {
        private readonly CollectionReference _films = database.Collection(collectionName);


        public async Task<IReadOnlyList<Film>> GetPageAsync(
            int limit, string? lastDocId, int? genreId, string? search, string? sortBy, string? sortDir)
        {
            var query = await MediaPageQuery.BuildAsync(
                _films, limit, lastDocId, genreId, search, sortBy, sortDir);


            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Documents
                .Select(document => document.ConvertTo<FilmDocument>().ToEntity())
                .ToList();
        }

        public async Task<Film?> GetByImdbIdAsync(string imdbId)
        {
            var snapshot = await _films.WhereEqualTo("ImdbId", imdbId).Limit(1).GetSnapshotAsync();
            return snapshot.Documents.Count == 0
                ? null
                : snapshot.Documents[0].ConvertTo<FilmDocument>().ToEntity();
        }
    }
}
    

