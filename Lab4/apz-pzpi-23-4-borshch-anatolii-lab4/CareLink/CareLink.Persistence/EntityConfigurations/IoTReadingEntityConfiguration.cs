using CareLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareLink.Persistence.EntityConfigurations
{
    public class IoTReadingEntityConfiguration : IEntityTypeConfiguration<IoTReading>
    {
        public void Configure(EntityTypeBuilder<IoTReading> builder)
        {
            builder.Property(x => x.ReadDateTime).IsRequired();
            builder.Property(x => x.Pulse).IsRequired();
            builder.Property(x => x.ActivityLevel).IsRequired();
            builder.Property(x => x.Temperature).IsRequired();
        }
    }
}