using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Infrastructure.Persistence.Configurations
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Content).HasMaxLength(4000);
            builder.Property(m => m.AttachmentFileName).HasMaxLength(500);
            builder.Property(m => m.AttachmentContentType).HasMaxLength(200);
            builder.Property(m => m.Type).HasConversion<int>();
            builder.Property(m => m.IsSeen).HasDefaultValue(false);
            builder.HasOne(m => m.CreatedBy)
                .WithMany()
                .HasForeignKey(m => m.CreatedById)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
