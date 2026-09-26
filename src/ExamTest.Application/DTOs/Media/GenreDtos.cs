namespace ExamTest.Application.DTOs.Media;

/// <summary>Genre as returned by the API.</summary>
public class GenreDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<int> FilmIds { get; set; } = new();
    public List<int> SerialIds { get; set; } = new();
}

/// <summary>Request body for creating or replacing a genre. The id is assigned by the server / taken from the route.</summary>
public class SaveGenreDto
{
    public string Name { get; set; } = string.Empty;
    public List<int> FilmIds { get; set; } = new();
    public List<int> SerialIds { get; set; } = new();
}
