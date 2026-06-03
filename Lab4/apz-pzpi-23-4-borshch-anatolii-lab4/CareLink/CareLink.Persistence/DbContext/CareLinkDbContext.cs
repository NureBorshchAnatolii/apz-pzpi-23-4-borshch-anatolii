using CareLink.Domain.Entities;
using CareLink.Domain.Entities.SubEntities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace CareLink.Persistence.DbContext
{
    public class CareLinkDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public const string SeedAdminEmail = "admin@carelink.local";
        public const string SeedAdminPassword = "123123Ab";

        public CareLinkDbContext(DbContextOptions<CareLinkDbContext> options) :
            base(options)
        {
        }
        
        public DbSet<CognitiveExercise> CognitiveExercises { get; set; }
        public DbSet<CognitiveResult> CognitiveResults { get; set; }
        public DbSet<IoTReading> IotReadings { get; set; }
        public DbSet<IoTDevice> IotDevices { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Relatives> Relatives { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<User> Users { get; set; }
        
        public DbSet<CognitiveExerciseType> CognitiveExerciseTypes { get; set; }
        public DbSet<DeviceType> DeviceTypes { get; set; }
        public DbSet<Difficulty> Difficulties { get; set; }
        public DbSet<NotificationType> NotificationTypes { get; set; }
        public DbSet<RelationType> RelationTypes { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<SubscriptionPlanType> SubscriptionPlanTypes { get; set; }
        public DbSet<SubscriptionStatus> SubscriptionStatuses { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CareLinkDbContext).Assembly);
            
            AddStartUpEntities(modelBuilder);
            
            base.OnModelCreating(modelBuilder);
        }

        private void AddStartUpEntities(ModelBuilder modelBuilder)
        {
            var seedAdminPasswordHash = GenerateSeedPasswordHash(SeedAdminPassword);
            Console.WriteLine($"[CareLink Seed] Admin email: {SeedAdminEmail}; PasswordHash: {seedAdminPasswordHash}");

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Administrator" },
                new Role { Id = 2, Name = "Moderator" },
                new Role { Id = 3, Name = "Doctor" },
                new Role { Id = 4, Name = "Guardian" },
                new Role { Id = 5, Name = "Relative" }
            );
            
            modelBuilder.Entity<CognitiveExerciseType>().HasData(
                new CognitiveExerciseType { Id = 1, Name = "Memory" },
                new CognitiveExerciseType { Id = 2, Name = "Attention" },
                new CognitiveExerciseType { Id = 3, Name = "Logic" },
                new CognitiveExerciseType { Id = 4, Name = "Language" },
                new CognitiveExerciseType { Id = 5, Name = "Orientation" }
            );
            
            modelBuilder.Entity<DeviceType>().HasData(
                new DeviceType { Id = 1, Name = "Smart Bracelet" },
                new DeviceType { Id = 2, Name = "Fall Detector" },
                new DeviceType { Id = 3, Name = "GPS Tracker" },
                new DeviceType { Id = 4, Name = "Medical Sensor" }
            );
            
            modelBuilder.Entity<Difficulty>().HasData(
                new Difficulty { Id = 1, Name = "Easy" },
                new Difficulty { Id = 2, Name = "Medium" },
                new Difficulty { Id = 3, Name = "Hard" }
            );
            
            modelBuilder.Entity<NotificationType>().HasData(
                new NotificationType { Id = 1, Name = "System" },
                new NotificationType { Id = 2, Name = "Health Alert" },
                new NotificationType { Id = 3, Name = "Medication Reminder" },
                new NotificationType { Id = 4, Name = "Fall Detected" }
            );
            
            modelBuilder.Entity<RelationType>().HasData(
                new RelationType { Id = 1, Name = "Child" },
                new RelationType { Id = 2, Name = "Guardian" },
                new RelationType { Id = 3, Name = "Partner" }
            );
            
            modelBuilder.Entity<SubscriptionPlanType>().HasData(
                new SubscriptionPlanType { Id = 1, Name = "Free" },
                new SubscriptionPlanType { Id = 2, Name = "Basic" },
                new SubscriptionPlanType { Id = 3, Name = "Premium" }
            );
            
            modelBuilder.Entity<SubscriptionStatus>().HasData(
                new SubscriptionStatus { Id = 1, Name = "Active" },
                new SubscriptionStatus { Id = 2, Name = "Paused" },
                new SubscriptionStatus { Id = 3, Name = "Expired" },
                new SubscriptionStatus { Id = 4, Name = "Cancelled" }
            );

            modelBuilder.Entity<User>().HasData(
                new User()
                {
                    Id = 1,
                    FirstName = "System",
                    LastName = "Administrator",
                    RoleId = 1,
                    Email = SeedAdminEmail,
                    PasswordHash = seedAdminPasswordHash,
                    DateOdBirth = new DateTime(1990, 1, 1),
                    Address = "Kyiv, Admin Street 1",
                    PhoneNumber = "+380501111111"
                });
        }

        public static string GenerateSeedPasswordHash(string password)
        {
            const int saltSize = 16;
            const int keySize = 32;
            const int iterations = 10000;

            var seedBytes = SHA256.HashData(Encoding.UTF8.GetBytes("carelink-seed-admin-salt"));
            var salt = new byte[saltSize];
            Array.Copy(seedBytes, salt, saltSize);

            using var algorithm = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var key = algorithm.GetBytes(keySize);

            return $"{iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
        }
    }
}