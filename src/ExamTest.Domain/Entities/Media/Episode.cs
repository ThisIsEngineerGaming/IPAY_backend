using Google.Cloud.Firestore;

namespace ExamTest.Domain.Entities.Media
{
    [FirestoreData]
    public class Episode
    {
        [FirestoreProperty] 
        public int Id { get; set; }
        [FirestoreProperty]
        public string Name { get; set; } = string.Empty;
        [FirestoreProperty]
        public string Description { get; set; } = string.Empty;
        [FirestoreProperty]
        public double Rating { get; set; }
        [FirestoreProperty]
        public string Director { get; set; } = string.Empty;
        [FirestoreProperty]
        public string PosterUrl { get; set; } = string.Empty;
        [FirestoreProperty]
        public string VideoUrl { get; set; } = string.Empty;
        [FirestoreProperty]
        public int SerialId { get; set; }
    }
}
