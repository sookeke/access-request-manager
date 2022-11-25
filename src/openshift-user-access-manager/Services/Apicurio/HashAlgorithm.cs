

using System.Runtime.Serialization;
namespace UserAccessManager.Services.Apicurio
{
    /// <summary>
    /// Enumerates all supported hash algorithms
    /// </summary>
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]

    public enum HashAlgorithm
    {
        /// <summary>
        /// Indicates a SHA256 hash
        /// </summary>
        [EnumMember(Value = "SHA256")]
        SHA256,
        /// <summary>
        /// Indicates an MD5 hash
        /// </summary>
        [EnumMember(Value = "MD5")]
        MD5
    }
}