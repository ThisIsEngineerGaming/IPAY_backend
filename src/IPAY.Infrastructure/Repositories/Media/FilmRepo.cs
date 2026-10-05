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
            Query query = _films;

            if (genreId.HasValue)
                query = query.WhereArrayContains("GenreIds", genreId.Value);

            var term = search?.Trim().ToLowerInvariant();
            if (!string.IsNullOrEmpty(term))
            {
                // Prefix search: range on NameLower, so it also has to be the first ordering.
                query = query
                    .WhereGreaterThanOrEqualTo("NameLower", term)
                    .WhereLessThanOrEqualTo("NameLower", term + "\uf8ff")
                    .OrderBy("NameLower");
            }
            else
            {
                var field = sortBy?.Trim().ToLowerInvariant() switch
                {
                    "rating" => "Rating",
                    "year" => "Year",
                    "date" => "CreatedAt",
                    _ => null
                };

                if (field is not null)
                {
                    var descending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
                    query = descending ? query.OrderByDescending(field) : query.OrderBy(field);
                }
            }

            // Document id as the final tie-breaker, so rows that share a rating/year/name
            // are never skipped or repeated across pages.
            query = query.OrderBy(FieldPath.DocumentId).Limit(limit);

            if (!string.IsNullOrEmpty(lastDocId))
            {
                var lastSnapshot = await _films.Document(lastDocId).GetSnapshotAsync();
                if (lastSnapshot.Exists)
                    query = query.StartAfter(lastSnapshot);
            }

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
