namespace ExamTest.Infastructure.Cloudinary
{
    /// <summary>
    /// Bound from the "Cloudinary" section of appsettings. Values come from the
    /// Cloudinary Console (Dashboard > API Environment variable).
    /// </summary>
    public class CloudinaryOptions
    {
        public const string SectionName = "Cloudinary";

        /// <summary>The Cloudinary cloud name, e.g. pcdocnud.</summary>
        public string CloudName { get; set; } = string.Empty;

        /// <summary>The Cloudinary API key.</summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// The Cloudinary API secret. Treat like any other credential - keep it out of
        /// source control in real deployments (env var / user-secrets / key vault).
        /// </summary>
        public string ApiSecret { get; set; } = string.Empty;
    }
}
