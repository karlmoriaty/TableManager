using System.IO.Compression;
using TableManager.DAL.Database;

namespace TableManager.BLL.Services;

public class DatabaseService
{
    private readonly string _dbPath;

    public DatabaseService(string? dbPath = null)
    {
        _dbPath = dbPath ?? DatabaseManager.GetDatabasePath();
    }

    public Task<string> BackupDatabaseAsync(string? backupDirectory = null)
    {
        if (string.IsNullOrEmpty(backupDirectory))
        {
            backupDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
        }

        if (!Directory.Exists(backupDirectory))
        {
            Directory.CreateDirectory(backupDirectory);
        }

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupFileName = $"tablemanager_backup_{timestamp}.db";
        var backupFilePath = Path.Combine(backupDirectory, backupFileName);

        File.Copy(_dbPath, backupFilePath, true);

        return Task.FromResult(backupFilePath);
    }

    public async Task<string> BackupDatabaseCompressedAsync(string? backupDirectory = null)
    {
        var dbBackupPath = await BackupDatabaseAsync(backupDirectory);
        
        var zipFileName = Path.ChangeExtension(Path.GetFileName(dbBackupPath), ".zip");
        var zipFilePath = Path.Combine(Path.GetDirectoryName(dbBackupPath) ?? "", zipFileName);

        if (File.Exists(zipFilePath))
        {
            File.Delete(zipFilePath);
        }

        using (var zip = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
        {
            zip.CreateEntryFromFile(dbBackupPath, Path.GetFileName(dbBackupPath));
        }

        File.Delete(dbBackupPath);

        return zipFilePath;
    }

    public Task RestoreDatabaseAsync(string backupFilePath)
    {
        if (!File.Exists(backupFilePath))
        {
            throw new FileNotFoundException("备份文件不存在", backupFilePath);
        }

        var fileInfo = new FileInfo(backupFilePath);

        if (fileInfo.Extension.ToLower() == ".zip")
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                Directory.CreateDirectory(tempDir);
                ZipFile.ExtractToDirectory(backupFilePath, tempDir);

                var dbFiles = Directory.GetFiles(tempDir, "*.db");
                if (dbFiles.Length == 0)
                {
                    throw new InvalidOperationException("压缩包中未找到数据库文件");
                }

                File.Copy(dbFiles[0], _dbPath, true);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }
        else
        {
            File.Copy(backupFilePath, _dbPath, true);
        }
        return Task.CompletedTask;
    }

    public List<string> GetBackupFiles(string? backupDirectory = null)
    {
        if (string.IsNullOrEmpty(backupDirectory))
        {
            backupDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
        }

        if (!Directory.Exists(backupDirectory))
        {
            return new List<string>();
        }

        var backupFiles = Directory.GetFiles(backupDirectory, "*.db")
            .Concat(Directory.GetFiles(backupDirectory, "*.zip"))
            .OrderByDescending(f => File.GetCreationTime(f))
            .ToList();

        return backupFiles;
    }

    public Task DeleteBackupAsync(string backupFilePath)
    {
        if (File.Exists(backupFilePath))
        {
            File.Delete(backupFilePath);
        }
        return Task.CompletedTask;
    }

    public long GetDatabaseSize()
    {
        if (File.Exists(_dbPath))
        {
            return new FileInfo(_dbPath).Length;
        }
        return 0;
    }
}
