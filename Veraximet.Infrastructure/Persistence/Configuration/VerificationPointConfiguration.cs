using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Veraximet.Domain.Entitys;

namespace Veraximet.Infrastructure.Persistence.Configuration
{
    public sealed class VerificationPointConfiguration : IEntityTypeConfiguration<VerificationPoint>
    {
        public void Configure(EntityTypeBuilder<VerificationPoint> builder)
        {
            builder.ToTable("VerificationPoints");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Property(x => x.Description).HasMaxLength(300);
            builder.HasIndex(x => new { x.VerificationId, x.Order }).IsUnique();
            builder.HasMany(x => x.Measurements).WithOne().HasForeignKey(x => x.VerificationPointId).OnDelete(DeleteBehavior.Cascade);
        }
    }

}
