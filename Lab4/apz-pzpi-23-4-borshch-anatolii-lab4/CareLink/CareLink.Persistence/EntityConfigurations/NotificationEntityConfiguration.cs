using CareLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareLink.Persistence.EntityConfigurations
{
    public class NotificationEntityConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.Property(x => x.CreatedDate).IsRequired();
            builder.Property(x => x.Content).IsRequired();
            builder.Property(x => x.IsRead).IsRequired();
            builder.Property(x => x.GroupOfIds).IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.UserId);

            builder.HasOne(x => x.Type)
                .WithMany()
                .HasForeignKey(x => x.NotificationTypeId);
        }
    }
}