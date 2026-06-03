using CareLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareLink.Persistence.EntityConfigurations
{
    public class IoTDeviceEntityConfiguration : IEntityTypeConfiguration<IoTDevice>
    {
        public void Configure(EntityTypeBuilder<IoTDevice> builder)
        {
            builder.Property(x => x.SerialNumber)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasOne(x => x.User)
                .WithMany(x => x.IoTDevices)
                .HasForeignKey(x => x.UserId);

            builder.HasOne(x => x.DeviceType)
                .WithMany()
                .HasForeignKey(x => x.DeviceTypeId);

            builder.HasMany(x => x.Readings)
                .WithOne(x => x.IoTDevice)
                .HasForeignKey(x => x.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}