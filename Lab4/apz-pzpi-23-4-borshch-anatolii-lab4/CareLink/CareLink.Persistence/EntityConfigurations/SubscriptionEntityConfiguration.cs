using CareLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareLink.Persistence.EntityConfigurations
{
    public class SubscriptionEntityConfiguration : IEntityTypeConfiguration<Subscription>
    {
        public void Configure(EntityTypeBuilder<Subscription> builder)
        {
            builder.Property(x => x.StripeSubscriptionId)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.StartedAt)
                .IsRequired();

            builder.Property(x => x.ExpiresAt)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.SubscriptionStatus)
                .WithMany()
                .HasForeignKey(x => x.SubscriptionStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SubscriptionType)
                .WithMany()
                .HasForeignKey(x => x.SubscriptionTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}