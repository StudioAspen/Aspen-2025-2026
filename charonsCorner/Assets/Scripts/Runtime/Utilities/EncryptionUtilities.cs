using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CharonsCorner.Runtime
{
    public static class EncryptionUtilities
    {
        private static readonly byte[] key;
        private static readonly byte[] initializationVector;

        static EncryptionUtilities()
        {
            string passphrase = "YourMomHahahahaha!!!!!";
            GetKeyAndIV(passphrase, out key, out initializationVector);
        }

        /// <summary>
        /// Derives a 32-byte AES key and 16-byte IV from the given passphrase.
        /// </summary>
        public static void GetKeyAndIV(string passphrase, out byte[] key, out byte[] iv)
        {
            // Hash the passphrase using SHA256
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(passphrase));

                // First 32 bytes = key
                key = new byte[32];
                Array.Copy(hash, 0, key, 0, 32);

                // Derive IV using second SHA256 of reversed string (for separation)
                byte[] reversed = Encoding.UTF8.GetBytes(new string(passphrase.Reverse().ToArray()));
                byte[] ivHash = sha.ComputeHash(reversed);
                iv = new byte[16];
                Array.Copy(ivHash, 0, iv, 0, 16);
            }
        }

        public static string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = initializationVector;

            using var encryptor = aes.CreateEncryptor();
            byte[] input = Encoding.UTF8.GetBytes(plainText);
            byte[] encrypted = encryptor.TransformFinalBlock(input, 0, input.Length);
            return Convert.ToBase64String(encrypted);
        }

        public static string Decrypt(string cipherText)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = initializationVector;

            using var decryptor = aes.CreateDecryptor();
            byte[] input = Convert.FromBase64String(cipherText);
            byte[] decrypted = decryptor.TransformFinalBlock(input, 0, input.Length);
            return Encoding.UTF8.GetString(decrypted);
        }
    }
}
