namespace ExamTest.Infastructure.Firebase
{
    /// <summary>
    /// Bound from the "Firebase" section of appsettings. Mirrors what you'd pass into
    /// admin.initializeApp({...}) on the Node side, minus the secret itself.
    /// </summary>
    public class FirebaseOptions
    {
        public const string SectionName = "Firebase";

        /// <summary>The Google Cloud/Firebase project ID, e.g. ipaygroup.</summary>
        public string ProjectId { get; set; } = string.Empty;

        /// <summary>
        /// Path to the downloaded service-account JSON (Project settings > Service accounts
        /// > Generate new private key). Relative paths are resolved from the content root.
        /// NEVER commit this file - keep it out of source control.
        /// </summary>
        public string ServiceAccountKeyPath { get; set; } = string.Empty;
    }
}
