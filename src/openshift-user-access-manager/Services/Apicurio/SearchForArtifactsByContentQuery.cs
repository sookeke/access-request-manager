namespace UserAccessManager.Services.Apicurio
{
    public class SearchForArtifactsByContentQuery
    {
        /// <summary>
        /// Gets/sets a parameter that can be set to true to indicate that the server should "canonicalize" the content when searching for matching artifacts. Canonicalization is unique to each artifact type, but typically involves removing any extra whitespace and formatting the content in a consistent manner. Must be used along with the artifactType query parameter.
        /// </summary>
        [Newtonsoft.Json.JsonProperty("canonical")]
        [System.Text.Json.Serialization.JsonPropertyName("canonical")]
        public virtual bool IsCannonical { get; set; }

        /// <summary>
        /// Gets/sets the type of artifact represented by the content being used for the search. This is only needed when using the canonical query parameter, so that the server knows how to canonicalize the content prior to searching for matching artifacts.
        /// </summary>
        [Newtonsoft.Json.JsonProperty("artifactType")]
        [System.Text.Json.Serialization.JsonPropertyName("artifactType")]
        public virtual ArtifactType ArtifactType { get; set; }

        /// <summary>
        /// Gets/sets the number of artifacts to skip before starting to collect the result set. Defaults to 0.
        /// </summary>
        [Newtonsoft.Json.JsonProperty("offset")]
        [System.Text.Json.Serialization.JsonPropertyName("offset")]
        public virtual long Offset { get; set; } = 0;

        /// <summary>
        /// Gets/sets the number of artifacts to return. Defaults to 20.
        /// </summary>
        [Newtonsoft.Json.JsonProperty("limit")]
        [System.Text.Json.Serialization.JsonPropertyName("limit")]
        public virtual long Limit { get; set; } = 20;

        /// <summary>
        /// Gets/sets the number of artifacts to return. Defaults to 20.
        /// </summary>
        [Newtonsoft.Json.JsonProperty("order")]
        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public virtual SortOrder Order { get; set; }

        /// <summary>
        /// Gets/sets the field to sort by. Can be one of: name and createdOn
        /// </summary>
        [Newtonsoft.Json.JsonProperty("orderby")]
        [System.Text.Json.Serialization.JsonPropertyName("orderby")]
        public virtual string? OrderBy { get; set; }
    }
}