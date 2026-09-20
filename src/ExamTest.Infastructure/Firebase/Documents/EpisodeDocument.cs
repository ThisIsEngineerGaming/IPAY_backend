using ExamTest.Domain.Entities.Media;
using Google.Cloud.Firestore;

namespace ExamTest.Infastructure.Firebase.Documents;

/// <summary>Firestore storage shape of <see cref="Episode"/>. Field names match the existing documents.</summary>
[FirestoreData]
public class EpisodeDocument
{
    [FirestoreProperty] public int Id { get; set; }
    [FirestoreProperty] public string Name { get; set; } = string.Empty;
    [FirestoreProperty] public string Description { get; set; } = string.Empty;
    [FirestoreProperty] public double Rating { get; set; }
    [FirestoreProperty] public string Director { get; set; } = string.Empty;
    [FirestoreProperty] public string PosterUrl { get; set; } = string.Empty;
    [FirestoreProperty] public string VideoUrl { get; set; } = string.Empty;
    [FirestoreProperty] public int SerialId { get; set; }

    public static EpisodeDocument FromEntity(Episode entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        Rating = entity.Rating,
        Director = entity.Director,
        PosterUrl = entity.PosterUrl,
        VideoUrl = entity.VideoUrl,
        SerialId = entity.SerialId
    };

    public Episode ToEntity() => new()
    {
        Id = Id,
        Name = Name,
        Description = Description,
        Rating = Rating,
        Director = Director,
        PosterUrl = PosterUrl,
        VideoUrl = VideoUrl,
        SerialId = SerialId
    };
}
