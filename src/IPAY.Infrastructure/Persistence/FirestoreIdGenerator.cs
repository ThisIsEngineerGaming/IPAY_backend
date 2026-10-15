using Google.Cloud.Firestore;

namespace IPAY.Infrastructure.Persistence;

/// <summary>
/// Hands out the next integer id for a collection using one small counter document
/// (<c>_counters/{collectionName}</c>) instead of reading the whole collection on every insert.
/// <para>
/// Cost per insert: 2 reads (the counter + a check that the candidate id is free) and 1 write,
/// no matter how big the collection is.
/// </para>
/// </summary>
internal static class FirestoreIdGenerator
{
    private const string CounterCollection = "_counters";
    private const string CounterField = "LastId";
    private const int NeedsSeed = -1;
    private const int MaxCollisionSkips = 100;

    /// <param name="getMaxExistingIdAsync">
    /// Returns the highest id already in the collection (0 when empty). Called at most once per
    /// collection, the first time its counter is needed, so existing data keeps its ids.
    /// </param>
    public static async Task<int> NextIdAsync(
        FirestoreDb database,
        string collectionName,
        Func<Task<int>> getMaxExistingIdAsync)
    {
        var counterRef = database.Collection(CounterCollection).Document(collectionName);
        var collection = database.Collection(collectionName);
        int? seed = null;

        while (true)
        {
            var seedValue = seed;

            var id = await database.RunTransactionAsync(async transaction =>
            {
                var counter = await transaction.GetSnapshotAsync(counterRef);

                int last;
                if (counter.Exists) last = counter.GetValue<int>(CounterField);
                else if (seedValue.HasValue) last = seedValue.Value;
                else return NeedsSeed;

                // Skip ids that are already taken (e.g. a document created at a chosen id via Update).
                var candidate = last + 1;
                for (var attempt = 0; attempt < MaxCollisionSkips; attempt++)
                {
                    var existing = await transaction.GetSnapshotAsync(collection.Document(candidate.ToString()));
                    if (!existing.Exists)
                    {
                        transaction.Set(counterRef, new Dictionary<string, object> { [CounterField] = candidate });
                        return candidate;
                    }

                    candidate++;
                }

                throw new InvalidOperationException(
                    $"Could not find a free id in '{collectionName}' after {MaxCollisionSkips} tries.");
            });

            if (id != NeedsSeed) return id;

            // No counter yet: seed it from whatever is already stored, then run the transaction again.
            seed = await getMaxExistingIdAsync();
        }
    }
}
