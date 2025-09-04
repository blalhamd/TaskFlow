using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Infrastructure.Data.EntitiesConfiguration
{
    public class DeveloperConfiguration : IEntityTypeConfiguration<Developer>
    {
        public void Configure(EntityTypeBuilder<Developer> builder)
        {
            builder.ToTable("Developers").HasKey(t => t.Id);

            builder.Property(x => x.FullName)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(x => x.JobTitle)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(x => x.ImagePath)
                   .HasMaxLength(256)
                   .IsRequired(false);

            builder.Property(x => x.JobLevel)
                   .HasConversion(
                      v => v.ToString(), // Convert enum to string for saving
                      v => (JobLevel)Enum.Parse(typeof(JobLevel), v) // Convert string back to enum for reading
                   );
        }
    }
}
