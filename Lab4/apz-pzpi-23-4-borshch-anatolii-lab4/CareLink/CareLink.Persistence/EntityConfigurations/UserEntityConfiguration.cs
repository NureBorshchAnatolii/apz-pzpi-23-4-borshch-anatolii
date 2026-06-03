using CareLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareLink.Persistence.EntityConfigurations
{
    public class UserEntityConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.PasswordHash)
                .IsRequired();

            builder.Property(x => x.PhoneNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Address)
                .IsRequired();

            builder.Property(x => x.DateOdBirth)
                .IsRequired();

            builder.Property(x => x.DateCreated)
                .IsRequired();

            builder.HasOne(x => x.Role)
                .WithMany()
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Guardians)
                .WithOne(x => x.RelativeUser)
                .HasForeignKey(x => x.RelativeUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.RelativeOf)
                .WithOne(x => x.GuardianUser)
                .HasForeignKey(x => x.GuardianUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.IoTDevices)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Notifications)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany<Subscription>()
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}