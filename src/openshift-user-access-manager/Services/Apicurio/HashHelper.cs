using System.Text;
namespace UserAccessManager.Services.Apicurio
{
    public static class HashHelper
    {
        /// <summary>
        /// Generates a new hash using the specified <see cref="HashAlgorithm"/>
        /// </summary>
        /// <param name="hashAlgorithm">The <see cref="HashAlgorithm"/> to use</param>
        /// <param name="value">The value to hash</param>
        /// <returns>The hashed value</returns>
        public static string Hash(HashAlgorithm hashAlgorithm, string value)
        {
            return hashAlgorithm switch
            {
                HashAlgorithm.SHA256 => SHA256Hash(value),
                HashAlgorithm.MD5 => MD5Hash(value),
                _ => throw new NotSupportedException($"The specified {nameof(HashAlgorithm)} '{hashAlgorithm}' is not supported")
            };
        }

        /// <summary>
        /// Generates a new SHA256 hash
        /// </summary>
        /// <param name="value">The value to hash</param>
        /// <returns>The hashed value</returns>
        public static string SHA256Hash(string value)
        {
            return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
        }

        /// <summary>
        /// Generates a new MD5 hash
        /// </summary>
        /// <param name="value">The value to hash</param>
        /// <returns>The hashed value</returns>
        public static string MD5Hash(string value)
        {
            return Convert.ToHexString(System.Security.Cryptography.MD5.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
        }
    }
}