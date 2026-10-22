using System.Collections.Generic;

namespace IPAY.Domain.Entities.Media
{
    //series class
    public class Series
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Year { get; set; }
        public double Rating { get; set; }
        public string Director { get; set; } = string.Empty;
        public string AgeRating { get; set; } = string.Empty;
        public string PosterUrl { get; set; } = string.Empty;
        public List<int> EpisodeIds { get; set; } = new();//can have several episodes
        public List<int> GenreIds { get; set; } = new();//also genres
        public DateTime CreatedAt { get; set; }
    }
}
