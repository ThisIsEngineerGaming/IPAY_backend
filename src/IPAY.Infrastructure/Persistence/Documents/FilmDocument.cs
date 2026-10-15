using IPAY.Domain.Entities.Media;
using Google.Cloud.Firestore;

namespace IPAY.Infrastructure.Persistence.Documents;

/// <summary>Firestore storage shape of <see cref="Film"/>. Field names match the existing documents.</summary>
[FirestoreData]
public class FilmDocument
{
    [FirestoreProperty] public int Id { get; set; }
    [FirestoreProperty] public string Name { get; set; } = string.Empty;
    [FirestoreProperty] public string Description { get; set; } = string.Empty;
    [FirestoreProperty] public int Year { get; set; }
    [FirestoreProperty] public double Rating { get; set; }
    [FirestoreProperty] public string Director { get; set; } = string.Empty;
    [FirestoreProperty] public string AgeRating { get; set; } = string.Empty;
    [FirestoreProperty] public string PosterUrl { get; set; } = string.Empty;
    [FirestoreProperty] public string VideoUrl { get; set; } = string.Empty;
    [FirestoreProperty] public List<int> GenreIds { get; set; } = new();
    [FirestoreProperty] public string ImdbId { get; set; } = string.Empty;
    /// <summary>Lower-cased Name, used for prefix search (Firestore has no case-insensitive match).</summary>
    [FirestoreProperty] public string NameLower { get; set; } = string.Empty;
    [FirestoreProperty] public DateTime CreatedAt { get; set; }

    public static FilmDocument FromEntity(Film entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        NameLower = (entity.Name ?? string.Empty).Trim().ToLowerInvariant(),
        Description = entity.Description,
        Year = entity.Year,
        Rating = entity.Rating,
        Director = entity.Director,
        AgeRating = entity.AgeRating,
        PosterUrl = entity.PosterUrl,
        VideoUrl = entity.VideoUrl,
        GenreIds = [.. entity.GenreIds],
        ImdbId = entity.ImdbId ?? string.Empty,
        CreatedAt = entity.CreatedAt == default ? DateTime.UtcNow : entity.CreatedAt
    };

    public Film ToEntity() => new()
    {
        Id = Id,
        Name = Name,
        Description = Description,
        Year = Year,
        Rating = Rating,
        Director = Director,
        AgeRating = AgeRating,
        PosterUrl = PosterUrl,
        VideoUrl = VideoUrl,
        GenreIds = [.. GenreIds ?? []],
        ImdbId = ImdbId ?? string.Empty,
        CreatedAt = CreatedAt
    };
}
