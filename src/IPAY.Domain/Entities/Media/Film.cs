using System.Collections.Generic;

namespace IPAY.Domain.Entities.Media
{
    public class Film
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
        /// IMDb id (e.g. tt0133093) when the film was imported from OMDb; empty otherwise. Used to block duplicate imports.
        public string ImdbId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<int> GenreIds { get; set; } = new();
    }
}
