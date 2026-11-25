using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Veraximet.Domain.Entitys;

namespace Veraximet.Infrastructure.Persistence.Configuration
{
    public sealed class MeasurementConfiguration : IEntityTypeConfiguration<Measurement>
    {
        public void Configure(EntityTypeBuilder<Measurement> builder)
        {
            builder.ToTable("Measurements");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.HasIndex(x => new { x.VerificationPointId, x.Order }).IsUnique();
        }
    }

}
