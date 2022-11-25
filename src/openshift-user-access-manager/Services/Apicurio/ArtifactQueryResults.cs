namespace UserAccessManager.Services.Apicurio
{
    public class ArtifactQueryResults
    {
        /// <summary>
        /// Gets/sets an <see cref="ICollection{T}"/> containing <see cref="Artifact"/> results
        /// </summary>
        [Newtonsoft.Json.JsonProperty("artifacts")]
        [System.Text.Json.Serialization.JsonPropertyName("artifacts")]
        public virtual ICollection<Artifact> Artifacts { get; set; } = new List<Artifact>();

        /// <summary>
        /// Gets/sets the result count
        /// </summary>
        [Newtonsoft.Json.JsonProperty("count")]
        [System.Text.Json.Serialization.JsonPropertyName("count")]
        public virtual long Count { get; set; }
    }
}