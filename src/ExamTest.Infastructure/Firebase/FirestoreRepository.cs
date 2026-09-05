using System.Reflection;
using ExamTest.Domain.Interfaces.ForRepos;
using Google.Cloud.Firestore;

namespace ExamTest.Infastructure.Firebase;

/// <summary>
/// Generic CRUD for one Cloud Firestore collection. The Firestore client uses the
/// Firebase service account, giving this server privileged access via Google IAM.
/// </summary>
public class FirestoreRepository<T>(FirestoreDb database, string collectionName) : IRepository<T> where T : class, new()
{
    private readonly CollectionReference _collection = database.Collection(collectionName);
    private static readonly PropertyInfo IdProperty = typeof(T).GetProperty("Id")
        ?? throw new InvalidOperationException($"{typeof(T).Name} must have a public 'Id' property.");

    public async Task<IReadOnlyList<T>> GetAllAsync()
    {
        var snapshot = await _collection.GetSnapshotAsync();
        return snapshot.Documents.Select(document => document.ConvertTo<T>()).ToList();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        var snapshot = await _collection.Document(id.ToString()).GetSnapshotAsync();
        return snapshot.Exists ? snapshot.ConvertTo<T>() : null;
    }

    public async Task<T> AddAsync(T entity)
    {
        var nextId = await GetNextIdAsync();
        IdProperty.SetValue(entity, nextId);
        await _collection.Document(nextId.ToString()).SetAsync(entity);
        return entity;
    }

    public async Task UpdateAsync(int id, T entity)
    {
        IdProperty.SetValue(entity, id);
        await _collection.Document(id.ToString()).SetAsync(entity);
    }

    public Task DeleteAsync(int id) => _collection.Document(id.ToString()).DeleteAsync();

    private async Task<int> GetNextIdAsync()
    {
        var all = await GetAllAsync();
        return all.Count == 0 ? 1 : all.Max(entity => (int)IdProperty.GetValue(entity)!) + 1;
    }
}
