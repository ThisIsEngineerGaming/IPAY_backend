using System.Collections.Generic;
using Google.Cloud.Firestore;

namespace ExamTest.Domain.Entities.Shop
{
    [FirestoreData]
    public class Category
    {
        [FirestoreProperty]
        public int Id { get; set; }
        [FirestoreProperty]
        public string Name { get; set; } = string.Empty;
        [FirestoreProperty]
        public int Quantity { get; set; }
        [FirestoreProperty]
        public List<int> ProductIds { get; set; } = new();
    }
}
