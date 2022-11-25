namespace UserAccessManager.Services.Apicurio
{
    public class ApiCurioRegistryClientOptions
    {
        /// <summary>
        /// Gets/sets the Api Curio Registry server uri
        /// </summary>
        public virtual Uri ServerUri { get; set; } = null!;

        /// <summary>
        /// Gets/sets the line ending format
        /// </summary>
        public virtual LineEndingFormatMode LineEndingFormatMode { get; set; } = LineEndingFormatMode.Preserve;
    }
}