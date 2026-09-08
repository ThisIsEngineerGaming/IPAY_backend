namespace ExamTest.Infastructure.Cloudinary
{
    /// <summary>
    /// Bound from the "Cloudinary" section of appsettings. Get these from your Cloudinary
    /// dashboard (cloudinary.com/console) - free tier covers a generous amount of image storage
    /// and bandwidth, which is why this exists instead of paid Firebase Storage.
    /// </summary>
    public class CloudinaryOptions
    {
        public const string SectionName = "Cloudinary";

        public string CloudName { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>Treat this like any other secret - keep it out of source control.</summary>
        public string ApiSecret { get; set; } = string.Empty;

        /// <summary>Folder inside your Cloudinary account that uploads get grouped into.</summary>
        public string Folder { get; set; } = "images";
    }
}
