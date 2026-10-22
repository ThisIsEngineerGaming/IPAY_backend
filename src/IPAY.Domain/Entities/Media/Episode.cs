namespace IPAY.Domain.Entities.Media
{
    //episodes(media part)
    public class Episode
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
}
