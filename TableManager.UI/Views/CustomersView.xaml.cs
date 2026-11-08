using System.Windows.Controls;

namespace TableManager.UI.Views;

public partial class CustomersView : UserControl
{
    public CustomersView()
    {
        InitializeComponent();
        Loaded += CustomersView_Loaded;
    }

    private void CustomersView_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ViewModels.CustomersViewModel viewModel)
        {
            viewModel.LoadCustomersCommand.Execute(null);
        }
    }

    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is ViewModels.CustomersViewModel viewModel)
        {
            viewModel.LoadContactsCommand.Execute(null);
        }
    }
}
