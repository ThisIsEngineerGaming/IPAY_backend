using System.Collections.Generic;

namespace ExamTest.Domain.Entities.Media
{
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
        public List<int> EpisodeIds { get; set; } = new();
        public List<int> GenreIds { get; set; } = new();
    }
}
