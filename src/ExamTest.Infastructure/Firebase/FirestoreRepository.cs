using System.Reflection;
using ExamTest.Domain.Interfaces.ForRepos;
using Google.Cloud.Firestore;

namespace ExamTest.Infastructure.Firebase;

/// <summary>
/// Generic CRUD for one Cloud Firestore collection. The Firestore client uses the
/// Firebase service account, giving this server privileged access via Google IAM.
/// <para>
/// Domain entities stay free of Firestore attributes: every read/write goes through a
/// <typeparamref name="TDocument"/> (the [FirestoreData] storage shape) and the supplied mapping delegates.
/// </para>
/// </summary>
public class FirestoreRepository<TEntity, TDocument>(
    FirestoreDb database,
    string collectionName,
    Func<TEntity, TDocument> toDocument,
    Func<TDocument, TEntity> toEntity) : IRepository<TEntity>
    where TEntity : class
    where TDocument : class, new()
{
    private readonly CollectionReference _collection = database.Collection(collectionName);
    private static readonly PropertyInfo IdProperty = typeof(TEntity).GetProperty("Id")
        ?? throw new InvalidOperationException($"{typeof(TEntity).Name} must have a public 'Id' property.");

    public async Task<IReadOnlyList<TEntity>> GetAllAsync()
    {
        var snapshot = await _collection.GetSnapshotAsync();
        return snapshot.Documents
            .Select(document => toEntity(document.ConvertTo<TDocument>()))
            .ToList();
    }

    public async Task<TEntity?> GetByIdAsync(int id)
    {
        var snapshot = await _collection.Document(id.ToString()).GetSnapshotAsync();
        return snapshot.Exists ? toEntity(snapshot.ConvertTo<TDocument>()) : null;
    }

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        var nextId = await GetNextIdAsync();
        IdProperty.SetValue(entity, nextId);
        await _collection.Document(nextId.ToString()).SetAsync(toDocument(entity));
        return entity;
    }

    public async Task UpdateAsync(int id, TEntity entity)
    {
        IdProperty.SetValue(entity, id);
        await _collection.Document(id.ToString()).SetAsync(toDocument(entity));
    }

    public Task DeleteAsync(int id) => _collection.Document(id.ToString()).DeleteAsync();

    private async Task<int> GetNextIdAsync()
    {
        var all = await GetAllAsync();
        return all.Count == 0
            ? 1
            : all.Max(entity => Convert.ToInt32(IdProperty.GetValue(entity) ?? 0)) + 1;
    }
}
