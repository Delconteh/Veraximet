using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Data.Sqlite;

namespace Veraximet.Infrastructure.Persistence
{

    public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<VeraximetDbContext>
    {
        public VeraximetDbContext CreateDbContext(string[] args)
        {
            var file = SqliteDatabasePath.Resolve();
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = file,
                ForeignKeys = true
            }.ToString();

            var options = new DbContextOptionsBuilder<VeraximetDbContext>().UseSqlite(connectionString).Options;

            return new VeraximetDbContext(options);
        }
    }

}
