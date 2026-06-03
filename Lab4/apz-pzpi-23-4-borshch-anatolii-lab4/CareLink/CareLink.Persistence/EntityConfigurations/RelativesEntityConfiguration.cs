using CareLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareLink.Persistence.EntityConfigurations
{
    public class RelativesEntityConfiguration : IEntityTypeConfiguration<Relatives>
    {
        public void Configure(EntityTypeBuilder<Relatives> builder)
        {
            builder.Property(x => x.AddedAt).IsRequired();

            builder.HasOne(x => x.RelationType)
                .WithMany()
                .HasForeignKey(x => x.RelationTypeId);

            builder.HasOne(x => x.RelativeUser)
                .WithMany(x => x.Guardians)
                .HasForeignKey(x => x.RelativeUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.GuardianUser)
                .WithMany(x => x.RelativeOf)
                .HasForeignKey(x => x.GuardianUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}