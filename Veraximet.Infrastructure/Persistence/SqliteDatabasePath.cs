using System.IO;

namespace Veraximet.Infrastructure.Persistence
{
    public static class SqliteDatabasePath
    {
        public static string Resolve(string? overridePath = null)
        {
            var configured = overridePath ?? Environment.GetEnvironmentVariable("VERAXIMET_DB_PATH");

            configured ??= Path.Combine( Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Veraximet", "veraximet.db");

            var fullPath = Path.GetFullPath(configured);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            return fullPath;
        }
    }

}
