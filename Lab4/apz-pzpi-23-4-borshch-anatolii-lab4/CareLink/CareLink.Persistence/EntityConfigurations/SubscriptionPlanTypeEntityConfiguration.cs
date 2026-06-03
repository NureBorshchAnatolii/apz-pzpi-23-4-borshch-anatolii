using CareLink.Domain.Entities.SubEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareLink.Persistence.EntityConfigurations
{
    public class SubscriptionPlanTypeEntityConfiguration : IEntityTypeConfiguration<SubscriptionPlanType>
    {
        public void Configure(EntityTypeBuilder<SubscriptionPlanType> builder)
        {
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);
        }
    }
}