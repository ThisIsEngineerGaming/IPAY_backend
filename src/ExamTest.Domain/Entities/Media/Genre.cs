using System.Collections.Generic;
using Google.Cloud.Firestore;

namespace ExamTest.Domain.Entities.Media
{
    [FirestoreData]
    public class Genre
    {
        [FirestoreProperty]
        public int Id { get; set; }
        [FirestoreProperty]
        public string Name { get; set; } = string.Empty;
        [FirestoreProperty]
        public List<int> FilmIds { get; set; } = new();
        [FirestoreProperty]
        public List<int> SerialIds { get; set; } = new();
    }
}
