using Google.Cloud.Firestore;

namespace IPAY.Infrastructure.Repositories.Media
{
    /// Paging / filtering / sorting shared by films and series (both have GenreIds, NameLower, Rating, Year, CreatedAt).
    internal static class MediaPageQuery
    {
        public static async Task<Query> BuildAsync(
            CollectionReference collection,
            int limit, string? lastDocId, int? genreId, string? search, string? sortBy, string? sortDir)
        {
            Query query = collection;

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
                var lastSnapshot = await collection.Document(lastDocId).GetSnapshotAsync();
                if (lastSnapshot.Exists)
                    query = query.StartAfter(lastSnapshot);
            }

            return query;
        }
    }
}
