using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Veraximet.Domain.Entitys;

namespace Veraximet.Infrastructure.Persistence.Configuration
{
    public sealed class InstrumentConfiguration : IEntityTypeConfiguration<Instrument>
    {
        public void Configure(EntityTypeBuilder<Instrument> builder)
        {
            builder.ToTable("Instruments");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(x => x.Code).IsRequired().HasMaxLength(40).UseCollation("NOCASE");
            builder.HasIndex(x => x.Code).IsUnique();
            builder.Property(x => x.Name).IsRequired().HasMaxLength(160);
            builder.Property(x => x.Category).IsRequired().HasMaxLength(80);
            builder.Property(x => x.VerificationIntervalMonths).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.Manufacturer).HasMaxLength(120);
            builder.Property(x => x.Model).HasMaxLength(120);
            builder.Property(x => x.SerialNumber).HasMaxLength(100);
            builder.Property(x => x.Department).HasMaxLength(100);
            builder.Property(x => x.Location).HasMaxLength(160);
            builder.Property(x => x.Notes).HasMaxLength(2000);
            builder.Property(x => x.OperationalStatus).HasConversion<string>().HasMaxLength(24).IsRequired();
            builder.Property(x => x.ConformityStatus).HasConversion<string>().HasMaxLength(24).IsRequired();
            builder.HasIndex(x => x.NextDueDate);
            builder.HasIndex(x => x.Category);
            builder.HasIndex(x => x.Department);
        }
    }
}
