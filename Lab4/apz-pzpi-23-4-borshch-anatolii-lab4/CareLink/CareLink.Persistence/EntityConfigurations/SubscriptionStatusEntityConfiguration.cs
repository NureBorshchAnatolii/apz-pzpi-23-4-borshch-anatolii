using CareLink.Domain.Entities.SubEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareLink.Persistence.EntityConfigurations
{
    public class SubscriptionStatusEntityConfiguration : IEntityTypeConfiguration<SubscriptionStatus>
    {
        public void Configure(EntityTypeBuilder<SubscriptionStatus> builder)
        {
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);
        }
    }
}