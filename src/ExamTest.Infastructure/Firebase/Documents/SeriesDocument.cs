using ExamTest.Domain.Entities.Media;
using Google.Cloud.Firestore;

namespace ExamTest.Infastructure.Firebase.Documents;

/// <summary>Firestore storage shape of <see cref="Series"/>. Field names match the existing documents.</summary>
[FirestoreData]
public class SeriesDocument
{
    [FirestoreProperty] public int Id { get; set; }
    [FirestoreProperty] public string Name { get; set; } = string.Empty;
    [FirestoreProperty] public string Description { get; set; } = string.Empty;
    [FirestoreProperty] public int Year { get; set; }
    [FirestoreProperty] public double Rating { get; set; }
    [FirestoreProperty] public string Director { get; set; } = string.Empty;
    [FirestoreProperty] public string AgeRating { get; set; } = string.Empty;
    [FirestoreProperty] public string PosterUrl { get; set; } = string.Empty;
    [FirestoreProperty] public List<int> EpisodeIds { get; set; } = new();
    [FirestoreProperty] public List<int> GenreIds { get; set; } = new();

    public static SeriesDocument FromEntity(Series entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        Year = entity.Year,
        Rating = entity.Rating,
        Director = entity.Director,
        AgeRating = entity.AgeRating,
        PosterUrl = entity.PosterUrl,
        EpisodeIds = [.. entity.EpisodeIds],
        GenreIds = [.. entity.GenreIds]
    };

    public Series ToEntity() => new()
    {
        Id = Id,
        Name = Name,
        Description = Description,
        Year = Year,
        Rating = Rating,
        Director = Director,
        AgeRating = AgeRating,
        PosterUrl = PosterUrl,
        EpisodeIds = [.. EpisodeIds ?? []],
        GenreIds = [.. GenreIds ?? []]
    };
}
