using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using KitchenEquipmentDemo.Enterprise.Data.Context;
using KitchenEquipmentDemo.Enterprise.Data.Models;

namespace KitchenEquipmentDemo.Enterprise.Seeder
{
    public static class StaffUsersSeed
    {
        private static readonly string[] MaleNames = new[]
        {
            "James Wilson", "Robert Johnson", "Michael Brown", "William Davis", "David Miller", "Richard Martinez",
            "Joseph Anderson", "Thomas Taylor", "Charles Thomas", "Christopher Moore", "Daniel Jackson", "Matthew White",
            "Anthony Harris", "Donald Martin", "Mark Thompson", "Paul Garcia", "Steven Martinez", "Andrew Robinson",
            "Kenneth Clark", "Joshua Rodriguez", "Kevin Lewis", "Brian Lee", "George Walker", "Edward Hall",
            "Ronald Allen", "Timothy Young", "Jason Hernandez", "Jeffrey King", "Ryan Wright", "Jacob Lopez",
            "Gary Hill", "Nicholas Scott", "Eric Green", "Stephen Adams", "Jonathan Baker", "Larry Gonzalez",
            "Justin Nelson", "Scott Carter", "Brandon Mitchell", "Benjamin Perez", "Samuel Roberts", "Frank Turner",
            "Raymond Phillips", "Patrick Campbell", "Alexander Parker", "Jack Evans", "Dennis Edwards", "Jerry Collins",
            "Tyler Stewart", "Aaron Sanchez", "Henry Morris", "Douglas Rogers", "Adam Reed", "Nathan Cook",
            "Peter Morgan", "Zachary Bell", "Kyle Murphy", "Walter Bailey", "Harold Rivera", "Arthur Cooper"
        };

        private static readonly string[] FemaleNames = new[]
        {
            "Mary Smith", "Patricia Johnson", "Jennifer Williams", "Linda Jones", "Elizabeth Brown", "Barbara Davis",
            "Susan Miller", "Jessica Wilson", "Sarah Moore", "Karen Taylor", "Nancy Anderson", "Lisa Thomas",
            "Margaret Jackson", "Betty White", "Sandra Harris", "Ashley Martin", "Dorothy Thompson", "Kimberly Garcia",
            "Emily Martinez", "Donna Robinson", "Michelle Clark", "Carol Rodriguez", "Amanda Lewis", "Melissa Lee",
            "Deborah Walker", "Stephanie Hall", "Rebecca Allen", "Sharon Young", "Laura Hernandez", "Cynthia King",
            "Kathleen Wright", "Amy Lopez", "Shirley Hill", "Angela Scott", "Helen Green", "Anna Adams",
            "Brenda Baker", "Pamela Gonzalez", "Nicole Nelson", "Ruth Carter", "Katherine Mitchell", "Samantha Perez",
            "Christine Roberts", "Emma Turner", "Catherine Phillips", "Debra Campbell", "Rachel Parker", "Carolyn Evans",
            "Janet Edwards", "Maria Collins", "Heather Stewart", "Diane Sanchez", "Olivia Morris", "Julie Rogers",
            "Joyce Reed", "Victoria Cook", "Kelly Morgan", "Christina Bell", "Lauren Murphy", "Joan Bailey"
        };

        public static void SeedStaffUsers()
        {
            var allNames = MaleNames.Concat(FemaleNames).ToList();
            var now = DateTime.UtcNow;
            int counter = 0;

            using (var db = new AppDbContext())
            using (var tx = db.Database.BeginTransaction())
            {
                int createdCount = 0;

                foreach (var fullName in allNames)
                {
                    var names = fullName.Split(' ');
                    if (names.Length != 2) continue;

                    var first = names[0];
                    var last = names[1];
                    var username = (first + "." + last).ToLower();
                    var email = $"{username}@demo.local";
                    var password = username;

                    if (db.User.Any(u => u.UserName == username))
                        continue;

                    var salt = CreateSalt(16);
                    var hash = DeriveHash(password, salt, 100_000, 32);


                    var user = new User
                    {
                        UserType = "Admin",
                        FirstName = first,
                        LastName = last,
                        EmailAddress = email,
                        UserName = username,
                        PasswordSalt = salt,
                        PasswordHash = hash,
                        PasswordAlgo = "PBKDF2-SHA256",
                        PasswordVersion = 1,
                        CreatedAt = now.AddSeconds(-createdCount),
                        CreatedBy = 1,
                        IsDeleted = false
                    };

                    db.User.Add(user);
                    counter++;
                }
                db.SaveChanges();
                tx.Commit();
                Console.WriteLine($"Seeded {createdCount} staff users.");
            }


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
