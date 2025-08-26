using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KitchenEquipmentDemo.Enterprise.Application.Helpers
{
    static class PasswordHelper
    {
        public static byte[] Hash(string password, byte[] salt)
        {
            var hash = DeriveHash(password, salt, 100_000, 32);

            return hash;
        }

        public static bool Verify(byte[] hash, byte[] salt, string password)
        {
            var hashPassword = DeriveHash(password, salt, 100_000, 32);

            // Manual constant-time comparison
            if (hash.Length != hashPassword.Length)
                return false;

            var result = 0;
            for (var i = 0; i < hash.Length; i++)
            {
                result |= hash[i] ^ hashPassword[i];
            }

            return result == 0;
        }
        private static byte[] CreateSalt(int size)
        {
            var salt = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        private static byte[] DeriveHash(string password, byte[] salt, int iterations, int size)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(size);
            }
        }
    }
}
