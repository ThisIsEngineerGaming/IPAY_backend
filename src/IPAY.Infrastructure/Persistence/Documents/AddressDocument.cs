using Google.Cloud.Firestore;
using IPAY.Domain.Entities.Shop;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPAY.Infrastructure.Persistence.Documents
{
    [FirestoreData]
    public class AddressDocument
    {
        [FirestoreProperty] public int Id { get; set; }
        [FirestoreProperty] public string Country { get; set; } = string.Empty;
        [FirestoreProperty] public string Street { get; set; } = string.Empty;
        [FirestoreProperty] public string City { get; set; } = string.Empty;
        [FirestoreProperty] public int UserId { get; set; }

        public static AddressDocument FromEntity(Address entity) => new()
        {
            Id = entity.Id,
            Country = entity.Country,
            Street = entity.Street,
            City = entity.City,
            UserId = entity.UserId
        };

        public Address ToEntity() => new()
        {
            Id = Id,
            Country = Country,
            Street = Street,
            City = City,
            UserId = UserId
        };
    }
}
