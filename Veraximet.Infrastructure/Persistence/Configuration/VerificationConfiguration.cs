using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Veraximet.Domain.Entitys;

namespace Veraximet.Infrastructure.Persistence.Configuration
{
    public sealed class VerificationConfiguration : IEntityTypeConfiguration<Verification>
    {
        public void Configure(EntityTypeBuilder<Verification> builder)
        {
            builder.ToTable("Verifications");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Property(x => x.PerformedBy).IsRequired().HasMaxLength(120);
            builder.Property(x => x.Unit).IsRequired().HasMaxLength(24);
            builder.Property(x => x.Notes).HasMaxLength(2000);
            builder.Property(x => x.DecisionRule).IsRequired().HasMaxLength(80);
            builder.Property(x => x.Result).HasConversion<string>().IsRequired().HasMaxLength(24);
            builder.HasOne<Instrument>().WithMany().HasForeignKey(x => x.InstrumentId).OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Points).WithOne().HasForeignKey(x => x.VerificationId).OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => new { x.InstrumentId, x.Date });
        }
    }

   
   
}
