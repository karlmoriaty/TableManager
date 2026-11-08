using System.IO;

namespace TableManager.DAL.Database;

public static class DatabaseManager
{
    private static DatabaseContext? _sharedContext;
    private static readonly object _lock = new();

    public static string GetDatabasePath()
    {
        string appDir = AppDomain.CurrentDomain.BaseDirectory;
        return Path.Combine(appDir, "tablemanager.db");
    }

    public static DatabaseContext GetSharedContext()
    {
        lock (_lock)
        {
            if (_sharedContext == null)
            {
                _sharedContext = new DatabaseContext(GetDatabasePath());
            }
            return _sharedContext;
        }
    }

    public static async Task InitializeAsync()
    {
        var context = GetSharedContext();
        await context.InitializeDatabaseAsync();
    }
}
