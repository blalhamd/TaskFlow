using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Infrastructure.Data.EntitiesConfiguration
{
    public class TaskEntityConfiguration : IEntityTypeConfiguration<TaskEntity>
    {
        public void Configure(EntityTypeBuilder<TaskEntity> builder)
        {
            builder.ToTable("Tasks").HasKey(x => x.Id);


            builder.Property(x => x.Content)
                   .HasMaxLength(1000)
                   .IsRequired();

            builder.Property(x => x.Progress)
                   .HasConversion(
                     v => v.ToString(),
                     v => (TaskProgress)Enum.Parse(typeof(TaskProgress), v)
                   );
        }
    }
}
