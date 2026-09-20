namespace ExamTest.Application.DTOs.Media;

/// <summary>Episode as returned by the API.</summary>
public class EpisodeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Rating { get; set; }
    public string Director { get; set; } = string.Empty;
    public string PosterUrl { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public int SerialId { get; set; }
}

/// <summary>Request body for creating or replacing an episode. The id is assigned by the server / taken from the route.</summary>
public class SaveEpisodeDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Rating { get; set; }
    public string Director { get; set; } = string.Empty;
    public string PosterUrl { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public int SerialId { get; set; }
}
