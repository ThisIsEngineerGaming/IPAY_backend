using System.Collections.Generic;

namespace ExamTest.Domain.Entities.Media
{
    public class Genre
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<int> FilmIds { get; set; } = new();
        public List<int> SerialIds { get; set; } = new();
    }
}
