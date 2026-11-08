using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TableManager.BLL.Services;

namespace TableManager.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ProductService _productService;
    private readonly CustomerService _customerService;
    private readonly OrderService _orderService;

    [ObservableProperty]
    private string _title = "TableManager - 企业级ERP/CRM管理系统";

    [ObservableProperty]
    private string _currentView = "Stock";

    [ObservableProperty]
    private int _productCount = 0;

    [ObservableProperty]
    private int _customerCount = 0;

    [ObservableProperty]
    private int _orderCount = 0;

    public MainViewModel()
    {
        _productService = new ProductService();
        _customerService = new CustomerService();
        _orderService = new OrderService();
        
        // 初始化时加载仪表盘数据
        LoadDashboardDataAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    private void NavigateTo(string viewName)
    {
        CurrentView = viewName;
    }

    [RelayCommand]
    private async Task LoadDashboardDataAsync()
    {
        var products = await _productService.GetAllProductsAsync();
        var customers = await _customerService.GetAllCustomersAsync();
        var orders = await _orderService.GetAllOrdersAsync();

        ProductCount = products.Count();
        CustomerCount = customers.Count();
        OrderCount = orders.Count();
    }
}
