using System.Text;

namespace UserAccessManager.Services.Apicurio
{
    public interface ISerializer
    {

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> containing all MIME types that can be serialized and deserialized by the <see cref="ISerializer"/>
        /// </summary>
        IEnumerable<string> SupportedMimeTypes { get; }

        /// <summary>
        /// Gets the <see cref="ISerializer"/>'s default MIME type
        /// </summary>
        string DefaultMimeType { get; }

        /// <summary>
        /// Gets the <see cref="ISerializer"/>'s default encoding
        /// </summary>
        Encoding DefaultEncoding { get; }

        /// <summary>
        /// Serializes a value to an output <see cref="Stream"/>
        /// </summary>
        /// <param name="value">The value to serialize</param>
        /// <param name="output">The output <see cref="Stream"/> to serialize the specified value to</param>
        void Serialize(object value, Stream output);

        /// <summary>
        /// Serializes a value to an output <see cref="Stream"/>
        /// </summary>
        /// <param name="value">The value to serialize</param>
        /// <param name="output">The output <see cref="Stream"/> to serialize the specified value to</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task SerializeAsync(object value, Stream output, CancellationToken cancellationToken = default);

        /// <summary>
        /// Serializes a value to an output <see cref="Stream"/>
        /// </summary>
        /// <param name="value">The value to serialize</param>
        /// <param name="output">The output <see cref="Stream"/> to serialize the specified value to</param>
        /// <param name="type">The type of the value to serialize</param>
        void Serialize(object value, Stream output, Type type);

        /// <summary>
        /// Serializes a value to an output <see cref="Stream"/>
        /// </summary>
        /// <param name="value">The value to serialize</param>
        /// <param name="output">The output <see cref="Stream"/> to serialize the specified value to</param>
        /// <param name="type">The type of the value to serialize</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        Task SerializeAsync(object value, Stream output, Type type, CancellationToken cancellationToken = default);

        /// <summary>
        /// Serializes a value to a byte array
        /// </summary>
        /// <param name="value">The value to serialize</param>
        /// <returns>A new byte array representing the serialized value</returns>
        byte[] Serialize(object value);

        /// <summary>
        /// Serializes a value to a byte array
        /// </summary>
        /// <param name="value">The value to serialize</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new byte array representing the serialized value</returns>
        Task<byte[]> SerializeAsync(object value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Serializes a value to a byte array
        /// </summary>
        /// <param name="value">The value to serialize</param>
        /// <param name="type">The type of the value to serialize</param>
        /// <returns>A new byte array representing the serialized value</returns>
        byte[] Serialize(object value, Type type);

        /// <summary>
        /// Serializes a value to a byte array
        /// </summary>
        /// <param name="value">The value to serialize</param>
        /// <param name="type">The type of the value to serialize</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new byte array representing the serialized value</returns>
        Task<byte[]> SerializeAsync(object value, Type type, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deserialize a value from an input <see cref="Stream"/>
        /// </summary>
        /// <typeparam name="T">The type of the value to deserialize</typeparam>
        /// <param name="input">The input <see cref="Stream"/> to deserialize the value from</param>
        /// <returns>The deserialized value</returns>
        T Deserialize<T>(Stream input);

        /// <summary>
        /// Deserialize a value from an input <see cref="Stream"/>
        /// </summary>
        /// <typeparam name="T">The type of the value to deserialize</typeparam>
        /// <param name="input">The input <see cref="Stream"/> to deserialize the value from</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The deserialized value</returns>
        Task<T> DeserializeAsync<T>(Stream input, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deserialize a value from an input byte array
        /// </summary>
        /// <typeparam name="T">The type of the value to deserialize</typeparam>
        /// <param name="input">The input byte array to deserialize the value from</param>
        /// <returns>The deserialized value</returns>
        T Deserialize<T>(byte[] input);

        /// <summary>
        /// Deserialize a value from an input byte array
        /// </summary>
        /// <typeparam name="T">The type of the value to deserialize</typeparam>
        /// <param name="input">The input byte array to deserialize the value from</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The deserialized value</returns>
        Task<T> DeserializeAsync<T>(byte[] input, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deserialize a value from an input <see cref="Stream"/>
        /// </summary>
        /// <param name="input">The input <see cref="Stream"/> to deserialize the value from</param>
        /// <param name="returnType">The type of the value to deserialize</param>
        /// <returns>The deserialized value</returns>
        object Deserialize(Stream input, Type returnType);

        /// <summary>
        /// Deserialize a value from an input <see cref="Stream"/>
        /// </summary>
        /// <param name="input">The input <see cref="Stream"/> to deserialize the value from</param>
        /// <param name="returnType">The type of the value to deserialize</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The deserialized value</returns>
        Task<object> DeserializeAsync(Stream input, Type returnType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deserialize a value from an input byte array
        /// </summary>
        /// <param name="input">The input byte array to deserialize the value from</param>
        /// <param name="returnType">The type of the value to deserialize</param>
        /// <returns>The deserialized value</returns>
        object Deserialize(byte[] input, Type returnType);

        /// <summary>
        /// Deserialize a value from an input byte array
        /// </summary>
        /// <param name="input">The input byte array to deserialize the value from</param>
        /// <param name="returnType">The type of the value to deserialize</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The deserialized value</returns>
        Task<object> DeserializeAsync(byte[] input, Type returnType, CancellationToken cancellationToken = default);

    }
}