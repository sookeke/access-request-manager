using System.Runtime.Serialization;

namespace UserAccessManager.Services.Apicurio
{
    /// <summary>
    /// Enumerates all supported artifact states
    /// </summary>
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]

    public enum ArtifactState
    {
        /// <summary>
        /// Indicates the the artifact is enabled
        /// </summary>
        [EnumMember(Value = "ENABLED")]
        Enabled,
        /// <summary>
        /// Indicates the the artifact is disabled
        /// </summary>
        [EnumMember(Value = "DISABLED")]
        Disabled,
        /// <summary>
        /// Indicates the the artifact has been deprecated
        /// </summary>
        [EnumMember(Value = "DEPRECATED")]
        Deprecated
    }
}