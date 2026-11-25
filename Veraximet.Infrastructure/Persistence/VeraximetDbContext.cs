using Microsoft.EntityFrameworkCore;
using Veraximet.Domain.Entitys;

namespace Veraximet.Infrastructure.Persistence
{

    public sealed class VeraximetDbContext : DbContext
    {
        public VeraximetDbContext(DbContextOptions<VeraximetDbContext> options) : base(options)
        {
        }

        public DbSet<Instrument> Instruments =>
            Set<Instrument>();

        public DbSet<Verification> Verifications => Set<Verification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(VeraximetDbContext).Assembly);
        }
    }
}
