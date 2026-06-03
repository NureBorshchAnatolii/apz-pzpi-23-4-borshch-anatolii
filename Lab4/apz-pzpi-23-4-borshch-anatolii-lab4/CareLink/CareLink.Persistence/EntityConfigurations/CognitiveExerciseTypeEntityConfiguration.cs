using CareLink.Domain.Entities.SubEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareLink.Persistence.EntityConfigurations
{
    public class CognitiveExerciseTypeEntityConfiguration : IEntityTypeConfiguration<CognitiveExerciseType>
    {
        public void Configure(EntityTypeBuilder<CognitiveExerciseType> builder)
        {
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);
        }
    }
}