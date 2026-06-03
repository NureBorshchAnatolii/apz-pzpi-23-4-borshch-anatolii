using CareLink.Domain.Entities.SubEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareLink.Persistence.EntityConfigurations
{
    public class DeviceTypeEntityConfiguration : IEntityTypeConfiguration<DeviceType>
    {
        public void Configure(EntityTypeBuilder<DeviceType> builder)
        {
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);
        }
    }
}