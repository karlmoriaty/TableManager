using System.Windows;
using System.Windows.Controls;

namespace TableManager.UI.Views;

public partial class OrdersView : UserControl
{
    public OrdersView()
    {
        InitializeComponent();
    }

    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.OrdersViewModel viewModel)
        {
            await viewModel.LoadDataCommand.ExecuteAsync(null);
        }
    }

    private async void OrdersGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (DataContext is ViewModels.OrdersViewModel viewModel && viewModel.SelectedOrder != null)
        {
            await viewModel.ViewOrderDetailCommand.ExecuteAsync(null);
        }
    }
}
