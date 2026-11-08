using System.Windows;
using TableManager.DAL.Database;

namespace TableManager.UI;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        try
        {
            await DatabaseManager.InitializeAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"数据库初始化失败: {ex.Message}\n\n详情: {ex.StackTrace}", 
                "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}


