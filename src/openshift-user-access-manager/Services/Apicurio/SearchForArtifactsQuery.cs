﻿namespace UserAccessManager.Services.Apicurio
{
    public class SearchForArtifactsQuery
    {
        /// <summary>
        /// Gets/sets the name to filter artifacts by
        /// </summary>
        [Newtonsoft.Json.JsonProperty("name")]
        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public virtual string? Name { get; set; }

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

        /// <summary>
        /// Gets/sets the label to filter by. Include one or more label to only return artifacts containing all of the specified labels.
        /// </summary>
        [Newtonsoft.Json.JsonProperty("labels")]
        [System.Text.Json.Serialization.JsonPropertyName("labels")]
        public virtual ICollection<string> Labels { get; set; } = new List<string>();

        /// <summary>
        /// Gets/sets the name/value properties to filter by. Separate each name/value pair using a colon. For example properties=foo:bar will return only artifacts with a custom property named foo and value bar.
        /// </summary>
        [Newtonsoft.Json.JsonProperty("properties")]
        [System.Text.Json.Serialization.JsonPropertyName("properties")]
        public virtual ICollection<string> Properties { get; set; } = new List<string>();

        /// <summary>
        /// Gets/sets the description to filter by
        /// </summary>
        [Newtonsoft.Json.JsonProperty("description")]
        [System.Text.Json.Serialization.JsonPropertyName("description")]
        public virtual string? Description { get; set; }

        /// <summary>
        /// Gets/sets the group to filter by
        /// </summary>
        [Newtonsoft.Json.JsonProperty("group")]
        [System.Text.Json.Serialization.JsonPropertyName("group")]
        public virtual string? Group { get; set; }

    }
}