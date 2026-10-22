using Google.Cloud.Firestore;
using IPAY.Domain.Entities.Shop;
using IPAY.Domain.Interfaces.ForRepos.Shop;
using IPAY.Infrastructure.Persistence;
using IPAY.Infrastructure.Persistence.Documents;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace IPAY.Infrastructure.Repositories.Shop
{
    //adress repository
    public class AddressRepository : IAddressRepository
    {

        private const string CollectionName = "addresses";

        private readonly FirestoreDb _database;

        private readonly CollectionReference _collection;
 
        private readonly PropertyInfo IdProperty;

        public AddressRepository(FirestoreDb firestoreDb)
        {
            _database = firestoreDb;
            _collection = firestoreDb.Collection(CollectionName);
            IdProperty = typeof(Address).GetProperty(nameof(Address.Id))!;
        }
        //get all
        public async Task<IReadOnlyList<Address>> GetAllAsync()
        {
            var snapshot = await _collection.GetSnapshotAsync();
            return snapshot.Documents
                .Select(document => toEntity(document.ConvertTo<AddressDocument>()))
                .ToList();
        }
        //get by id
        public async Task<Address?> GetByIdAsync(int id)
        {
            var snapshot = await _collection.Document(id.ToString()).GetSnapshotAsync();
            return snapshot.Exists ? toEntity(snapshot.ConvertTo<AddressDocument>()) : null;
        }
        //get user
        public async Task<IReadOnlyList<Address>> GetByUserIdAsync(int userId)
        {
            var snapshot = await _collection
                .WhereEqualTo("UserId", userId)
                .GetSnapshotAsync();

            return snapshot.Documents
                .Select(document => toEntity(document.ConvertTo<AddressDocument>()))
                .ToList();
        }
        // get by user id
        public async Task<Address?> GetByUserIdAndIdAsync(int userId, int addressId)
        {
            var snapshot = await _collection.Document(addressId.ToString()).GetSnapshotAsync();
            if (!snapshot.Exists) return null;

            var entity = toEntity(snapshot.ConvertTo<AddressDocument>());
            return entity.UserId == userId ? entity : null;
        }
        //add async
        public async Task<Address> AddAsync(Address entity)
        {
            var nextId = await GetNextIdAsync();
            IdProperty.SetValue(entity, nextId);
            await _collection.Document(nextId.ToString()).SetAsync(toDocument(entity));
            return entity;
        }
        //update async
        public async Task UpdateAsync(int id, Address entity)
        {
            IdProperty.SetValue(entity, id);
            await _collection.Document(id.ToString()).SetAsync(toDocument(entity));
        }
        //delete async
        public Task DeleteAsync(int id) =>
            _collection.Document(id.ToString()).DeleteAsync();


        // One counter document instead of reading every address for each insert.
        private Task<int> GetNextIdAsync() =>
            FirestoreIdGenerator.NextIdAsync(_database, CollectionName, GetMaxExistingIdAsync);

        private async Task<int> GetMaxExistingIdAsync()

        {
            var all = await GetAllAsync();
            return all.Count == 0
                ? 0
                : all.Max(entity => entity.Id);
        }

        // ===== Mappings =====
        private Address toEntity(AddressDocument document)
        {
            return new Address
            {
                Id = document.Id,
                Country = document.Country,
                Street = document.Street,
                City = document.City,
                UserId = document.UserId
            };
        }

        private AddressDocument toDocument(Address entity)
        {
            return new AddressDocument
            {
                Id = entity.Id,
                Country = entity.Country,
                Street = entity.Street,
                City = entity.City,
                UserId = entity.UserId
            };
        }
    }
}
