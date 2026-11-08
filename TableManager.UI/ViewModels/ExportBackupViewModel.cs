using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using TableManager.BLL.Services;

namespace TableManager.UI.ViewModels;

public partial class ExportBackupViewModel : ObservableObject
{
    private readonly ExportService _exportService;
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private ObservableCollection<string> _backupFiles = new();

    [ObservableProperty]
    private string? _selectedBackupFile;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _databaseSize;

    public ExportBackupViewModel()
    {
        _exportService = new ExportService();
        _databaseService = new DatabaseService();
        LoadBackupFiles();
        UpdateDatabaseSize();
    }

    [RelayCommand]
    private async Task ExportProductsAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "正在导出商品数据...";

            var data = await _exportService.ExportProductsAsync();
            
            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel 文件 (*.xlsx)|*.xlsx",
                FileName = $"商品列表_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                DefaultExt = ".xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                await File.WriteAllBytesAsync(saveDialog.FileName, data);
                StatusMessage = $"商品数据已导出到: {saveDialog.FileName}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"导出失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ExportCustomersAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "正在导出客户数据...";

            var data = await _exportService.ExportCustomersAsync();
            
            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel 文件 (*.xlsx)|*.xlsx",
                FileName = $"客户列表_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                DefaultExt = ".xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                await File.WriteAllBytesAsync(saveDialog.FileName, data);
                StatusMessage = $"客户数据已导出到: {saveDialog.FileName}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"导出失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ExportOrdersAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "正在导出订单数据...";

            var data = await _exportService.ExportOrdersAsync();
            
            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel 文件 (*.xlsx)|*.xlsx",
                FileName = $"订单列表_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                DefaultExt = ".xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                await File.WriteAllBytesAsync(saveDialog.FileName, data);
                StatusMessage = $"订单数据已导出到: {saveDialog.FileName}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"导出失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ExportStockRecordsAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "正在导出库存记录...";

            var data = await _exportService.ExportStockRecordsAsync();
            
            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel 文件 (*.xlsx)|*.xlsx",
                FileName = $"库存记录_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                DefaultExt = ".xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                await File.WriteAllBytesAsync(saveDialog.FileName, data);
                StatusMessage = $"库存记录已导出到: {saveDialog.FileName}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"导出失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task BackupDatabaseAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "正在备份数据库...";

            var backupPath = await _databaseService.BackupDatabaseAsync();
            StatusMessage = $"数据库已备份到: {backupPath}";
            LoadBackupFiles();
            UpdateDatabaseSize();
        }
        catch (Exception ex)
        {
            StatusMessage = $"备份失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task BackupDatabaseCompressedAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "正在压缩备份数据库...";

            var backupPath = await _databaseService.BackupDatabaseCompressedAsync();
            StatusMessage = $"数据库已压缩备份到: {backupPath}";
            LoadBackupFiles();
            UpdateDatabaseSize();
        }
        catch (Exception ex)
        {
            StatusMessage = $"压缩备份失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RestoreDatabaseAsync()
    {
        if (string.IsNullOrEmpty(SelectedBackupFile))
        {
            StatusMessage = "请先选择要恢复的备份文件";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "正在恢复数据库...";

            await _databaseService.RestoreDatabaseAsync(SelectedBackupFile);
            StatusMessage = $"数据库已从 {SelectedBackupFile} 恢复";
            UpdateDatabaseSize();
        }
        catch (Exception ex)
        {
            StatusMessage = $"恢复失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteBackupAsync()
    {
        if (string.IsNullOrEmpty(SelectedBackupFile))
        {
            StatusMessage = "请先选择要删除的备份文件";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "正在删除备份文件...";

            await _databaseService.DeleteBackupAsync(SelectedBackupFile);
            StatusMessage = $"备份文件 {SelectedBackupFile} 已删除";
            LoadBackupFiles();
            SelectedBackupFile = null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"删除失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void LoadBackupFiles()
    {
        try
        {
            var files = _databaseService.GetBackupFiles();
            BackupFiles.Clear();
            foreach (var file in files)
            {
                BackupFiles.Add(file);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载备份文件失败: {ex.Message}";
        }
    }

    private void UpdateDatabaseSize()
    {
        var size = _databaseService.GetDatabaseSize();
        DatabaseSize = FormatFileSize(size);
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }
}
