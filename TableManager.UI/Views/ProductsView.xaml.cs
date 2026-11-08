using System.Windows.Controls;
using TableManager.UI.ViewModels;

namespace TableManager.UI.Views;

public partial class ProductsView : UserControl
{
    public ProductsView()
    {
        InitializeComponent();
        Loaded += async (s, e) =>
        {
            if (DataContext is ProductsViewModel vm)
            {
                await vm.LoadProductsCommand.ExecuteAsync(null);
            }
        };
    }
}
