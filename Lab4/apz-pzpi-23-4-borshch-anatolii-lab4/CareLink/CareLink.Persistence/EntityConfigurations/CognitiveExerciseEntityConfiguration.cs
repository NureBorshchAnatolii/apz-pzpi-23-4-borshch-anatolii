using CareLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareLink.Persistence.EntityConfigurations
{
    public class CognitiveExerciseEntityConfiguration : IEntityTypeConfiguration<CognitiveExercise>
    {
        public void Configure(EntityTypeBuilder<CognitiveExercise> builder)
        {
            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .IsRequired();

            builder.HasOne(x => x.Difficulty)
                .WithMany()
                .HasForeignKey(x => x.DifficultyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Type)
                .WithMany()
                .HasForeignKey(x => x.TypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Results)
                .WithOne(x => x.CognitiveExercise)
                .HasForeignKey(x => x.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}