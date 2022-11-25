namespace UserAccessManager.Services.Apicurio
{
    public interface ITextSerializer : ISerializer
    {
        /// <summary>
        /// Serializes the specified value
        /// </summary>
        /// <param name="value">The value to serialize</param>
        /// <returns>The resulting text</returns>
        new string Serialize(object value);

        /// <summary>
        /// Serializes the specified value
        /// </summary>
        /// <param name="value">The value to serialize</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The resulting text</returns>
        new Task<string> SerializeAsync(object value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Serializes the specified value
        /// </summary>
        /// <param name="value">The value to serialize</param>
        /// <param name="type">The type of the value to serialize</param>
        /// <returns>The resulting text</returns>
        new string Serialize(object value, Type type);

        /// <summary>
        /// Serializes the specified value
        /// </summary>
        /// <param name="value">The value to serialize</param>
        /// <param name="type">The type of the value to serialize</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The resulting text</returns>
        new Task<string> SerializeAsync(object value, Type type, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deserializes the specified text
        /// </summary>
        /// <param name="input">The text to deserialize</param>
        /// <param name="returnType">The expected return type</param>
        /// <returns>The deserialized value</returns>
        object Deserialize(string input, Type returnType);

        /// <summary>
        /// Deserializes the specified text
        /// </summary>
        /// <param name="input">The text to deserialize</param>
        /// <param name="returnType">The expected return type</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The deserialized value</returns>
        Task<object> DeserializeAsync(string input, Type returnType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deserializes the specified text
        /// </summary>
        /// <typeparam name="T">The expected return type</typeparam>
        /// <param name="input">The text to deserialize</param>
        /// <returns>The deserialized value</returns>
        T Deserialize<T>(string input);

        /// <summary>
        /// Deserializes the specified text
        /// </summary>
        /// <typeparam name="T">The expected return type</typeparam>
        /// <param name="input">The text to deserialize</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The deserialized value</returns>
        Task<T> DeserializeAsync<T>(string input, CancellationToken cancellationToken = default);
    }
}