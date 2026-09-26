using ExamTest.Domain.Entities.Media;
using Google.Cloud.Firestore;

namespace ExamTest.Infastructure.Firebase.Documents;

/// <summary>Firestore storage shape of <see cref="Genre"/>. Field names match the existing documents.</summary>
[FirestoreData]
public class GenreDocument
{
    [FirestoreProperty] public int Id { get; set; }
    [FirestoreProperty] public string Name { get; set; } = string.Empty;
    [FirestoreProperty] public List<int> FilmIds { get; set; } = new();
    [FirestoreProperty] public List<int> SerialIds { get; set; } = new();

    public static GenreDocument FromEntity(Genre entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        FilmIds = [.. entity.FilmIds],
        SerialIds = [.. entity.SerialIds]
    };

    public Genre ToEntity() => new()
    {
        Id = Id,
        Name = Name,
        FilmIds = [.. FilmIds ?? []],
        SerialIds = [.. SerialIds ?? []]
    };
}
