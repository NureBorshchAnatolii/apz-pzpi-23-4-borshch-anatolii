using CareLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareLink.Persistence.EntityConfigurations
{
    public class CognitiveResultEntityConfiguration : IEntityTypeConfiguration<CognitiveResult>
    {
        public void Configure(EntityTypeBuilder<CognitiveResult> builder)
        {
            builder.Property(x => x.Score).IsRequired();

            builder.Property(x => x.CompletedAt).IsRequired();

            builder.HasOne(x => x.CognitiveExercise)
                .WithMany(x => x.Results)
                .HasForeignKey(x => x.ExerciseId);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}