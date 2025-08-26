// SuperAdminSeed.cs
// Run once from a tiny Console app, or call on WPF first launch (dev-only).

using System;
using System.Linq;
using System.Security.Cryptography;
using KitchenEquipmentDemo.Enterprise.Data.Context;
using KitchenEquipmentDemo.Enterprise.Data.Models;

namespace KitchenEquipmentDemo.Enterprise.Seeder
{
    public static class SuperAdminSeed
    {
        // Identity
        public const string UserName = "norman.super";
        public const string Email = "gonzales.norman@gmail.com";
        public const string FirstName = "Norman";
        public const string LastName = "Gonzales";

        // Password (rotate after first login!)
        public const string InitialPassword = "Duke!N0rm@n#2025$KE";

        // Hashing parameters
        public const string Algorithm = "PBKDF2-SHA256";
        public const int Iterations = 100_000;
        public const int SaltSize = 16;  // 128-bit
        public const int HashSize = 32;  // 256-bit (fits VARBINARY(64))

        public static void EnsureSuperAdmin()
        {
            using (var db = new AppDbContext())
            using (var tx = db.Database.BeginTransaction())
            {
                // NOTE: your Reverse POCO may expose db.User or db.Users; keep whichever your project uses.
                var existing = db.User.FirstOrDefault(u => u.UserName == UserName || u.EmailAddress == Email);
                if (existing != null)
                {
                    Console.WriteLine($"SuperAdmin already exists (UserId={existing.UserId}).");
                    return;
                }

                // Generate salt + hash
                var salt = CreateSalt(SaltSize);
                var hash = DeriveHash(InitialPassword, salt, Iterations, HashSize);

                // 1) Insert user (created_by cannot point to itself yet)
                var user = new User
                {
                    UserType = "SuperAdmin",
                    FirstName = FirstName,
                    LastName = LastName,
                    EmailAddress = Email,
                    UserName = UserName,
                    PasswordSalt = salt,
                    PasswordHash = hash,
                    PasswordAlgo = Algorithm,
                    PasswordVersion = 1,
                    CreatedAt = DateTime.UtcNow,
                    // CreatedBy will be set after SaveChanges when we have the new UserId
                    IsDeleted = false
                };

                db.User.Add(user);
                db.SaveChanges(); // assigns identity -> user.UserId

                // 2) Now that we have UserId, set created_by = self and persist
                user.CreatedBy = user.UserId;
                db.SaveChanges();

                tx.Commit();

                Console.WriteLine($"Created SuperAdmin: UserId={user.UserId}, UserName={user.UserName}");
                Console.WriteLine("IMPORTANT: Rotate this initial password on first login.");
            }
        }

        // --- Helpers ---------------------------------------------------------

        private static byte[] CreateSalt(int size)
        {
            var salt = new byte[size];
            using (var rng = RandomNumberGenerator.Create()) // or new RNGCryptoServiceProvider()
            {
                rng.GetBytes(salt); // fills with cryptographically secure random bytes
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

        // Optional: use this in your AuthService login path
        public static bool VerifyPassword(string password, byte[] salt, byte[] expectedHash,
                                          int iterations = Iterations, int size = HashSize)
        {
            var actual = DeriveHash(password, salt, iterations, size);
            return FixedTimeEquals(actual, expectedHash);
        }

        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}