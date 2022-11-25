using System.Runtime.Serialization;

namespace UserAccessManager.Services.Apicurio
{
    /// <summary>
    /// Enumerates all supported artifact types
    /// </summary>
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]

    public enum ArtifactType
    {
        /// <summary>
        /// Indicates an Avro schema
        /// </summary>
        [EnumMember(Value = "Avro")]
        Avro,
        /// <summary>
        /// Indicates a PROTOBUF schema
        /// </summary>
        [EnumMember(Value = "PROTOBUF")]
        PROTOBUF,
        /// <summary>
        /// Indicates a JSON schema
        /// </summary>
        [EnumMember(Value = "JSON")]
        JSON,
        /// <summary>
        /// Indicates an OPENAPI schema
        /// </summary>
        [EnumMember(Value = "OPENAPI")]
        OPENAPI,
        /// <summary>
        /// Indicates an ASYNCAPI schema
        /// </summary>
        [EnumMember(Value = "ASYNCAPI")]
        ASYNCAPI,
        /// <summary>
        /// Indicates a GRAPHQL schema
        /// </summary>
        [EnumMember(Value = "GRAPHQL")]
        GRAPHQL,
        /// <summary>
        /// Indicates a KCONNECT schema
        /// </summary>
        [EnumMember(Value = "KCONNECT")]
        KCONNECT,
        /// <summary>
        /// Indicates a WSDL schema
        /// </summary>
        [EnumMember(Value = "WSDL")]
        WSDL,
        /// <summary>
        /// Indicates an XSD schema
        /// </summary>
        [EnumMember(Value = "XSD")]
        XSD,
        /// <summary>
        /// Indicates an XML schema
        /// </summary>
        [EnumMember(Value = "XML")]
        XML
    }
}