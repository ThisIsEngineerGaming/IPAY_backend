namespace ExamTest.Application.DTOs.Media;

/// <summary>Film (stored in Firestore) as returned by the API.</summary>
public class FilmDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Year { get; set; }
    public double Rating { get; set; }
    public string Director { get; set; } = string.Empty;
    public string AgeRating { get; set; } = string.Empty;
    public string PosterUrl { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public List<int> GenreIds { get; set; } = new();
}

/// <summary>Request body for creating or replacing a film. The id is assigned by the server / taken from the route.</summary>
public class SaveFilmDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Year { get; set; }
    public double Rating { get; set; }
    public string Director { get; set; } = string.Empty;
    public string AgeRating { get; set; } = string.Empty;
    public string PosterUrl { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public List<int> GenreIds { get; set; } = new();
}
