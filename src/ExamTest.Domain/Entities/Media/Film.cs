using System.Collections.Generic;
using Google.Cloud.Firestore;

namespace ExamTest.Domain.Entities.Media
{
    [FirestoreData]
    public class Film
    {
        [FirestoreProperty]
        public int Id { get; set; }
        [FirestoreProperty]
        public string Name { get; set; } = string.Empty;
        [FirestoreProperty]
        public string Description { get; set; } = string.Empty;
        [FirestoreProperty]
        public int Year { get; set; }
        [FirestoreProperty]
        public double Rating { get; set; }
        [FirestoreProperty]
        public string Director { get; set; } = string.Empty;
        [FirestoreProperty]
        public string AgeRating { get; set; } = string.Empty;
        [FirestoreProperty]
        public string PosterUrl { get; set; } = string.Empty;
        [FirestoreProperty]
        public string VideoUrl { get; set; } = string.Empty;
        [FirestoreProperty]
        public List<int> GenreIds { get; set; } = new();
    }
}
