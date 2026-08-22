using System;
using System.Security.Cryptography;
using System.Text;

namespace ShiftDesk.Security
{
    /// <summary>
    /// Converts a password into the 64-character hexadecimal SHA-256 digest
    /// that gets stored in tbl_users.
    ///
    /// Only Data.UserStore calls this. Neither form ever touches a hash, which
    /// is deliberate - it removes any chance of hashing on the way in and then
    /// forgetting to hash on the way out.
    /// </summary>
    internal static class PasswordHasher
    {
        /// <summary>
        /// Same input always produces the same 64 characters. There is no
        /// reverse operation, which is the entire point: the database can check
        /// a password without ever holding one.
        /// </summary>
        internal static string Hash(string password)
        {
            byte[] input = Encoding.UTF8.GetBytes(password ?? string.Empty);

            using (SHA256 algorithm = SHA256.Create())
            {
                byte[] digest = algorithm.ComputeHash(input);

                // 32 bytes in, two hex characters out of each one, so 64 characters.
                return BitConverter.ToString(digest).Replace("-", string.Empty).ToLowerInvariant();
            }
        }
    }
}
