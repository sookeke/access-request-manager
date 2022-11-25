using System.Runtime.Serialization;

namespace UserAccessManager.Services.Apicurio
{
    /// <summary>
    /// Enumerates all supported sort orders
    /// </summary>
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public enum SortOrder
    {
        /// <summary>
        /// Indicates an ascending sorting
        /// </summary>
        [EnumMember(Value = "asc")]
        Ascending,
        /// <summary>
        /// Indicates a descending sorting
        /// </summary>
        [EnumMember(Value = "desc")]
        Descending
    }
}