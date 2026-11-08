using System.Windows.Controls;
using TableManager.UI.ViewModels;

namespace TableManager.UI.Views;

public partial class StockView : UserControl
{
    public StockView()
    {
        InitializeComponent();
        Loaded += async (s, e) =>
        {
            if (DataContext is StockViewModel vm)
            {
                await vm.LoadDataCommand.ExecuteAsync(null);
            }
        };
    }
}
