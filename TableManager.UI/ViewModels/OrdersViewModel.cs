using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using TableManager.BLL.Services;
using TableManager.DAL.Models;

namespace TableManager.UI.ViewModels;

public partial class OrdersViewModel : ObservableObject
{
    private readonly OrderService _orderService;
    private readonly ProductService _productService;
    private readonly CustomerService _customerService;

    [ObservableProperty]
    private ObservableCollection<Order> _orders = new ObservableCollection<Order>();

    [ObservableProperty]
    private ObservableCollection<Product> _products = new ObservableCollection<Product>();

    [ObservableProperty]
    private ObservableCollection<Customer> _customers = new ObservableCollection<Customer>();

    [ObservableProperty]
    private ObservableCollection<OrderItemViewModel> _currentOrderItems = new ObservableCollection<OrderItemViewModel>();

    [ObservableProperty]
    private Order? _selectedOrder;

    [ObservableProperty]
    private OrderDetail? _selectedOrderDetail;

    [ObservableProperty]
    private Customer? _selectedCustomer;

    [ObservableProperty]
    private Product? _selectedProduct;

    [ObservableProperty]
    private bool _isCreateOrderDialogOpen;

    [ObservableProperty]
    private bool _isOrderDetailDialogOpen;

    [ObservableProperty]
    private string? _orderNotes;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _showErrorMessage;

    public OrdersViewModel()
    {
        _orderService = new OrderService();
        _productService = new ProductService();
        _customerService = new CustomerService();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            var orders = await _orderService.GetAllOrdersAsync();
            var products = await _productService.GetAllProductsAsync();
            var customers = await _customerService.GetAllCustomersAsync();

            Orders = new ObservableCollection<Order>(orders);
            Products = new ObservableCollection<Product>(products);
            Customers = new ObservableCollection<Customer>(customers);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    [RelayCommand]
    private void OpenCreateOrderDialog()
    {
        CurrentOrderItems.Clear();
        SelectedCustomer = null;
        OrderNotes = null;
        IsCreateOrderDialogOpen = true;
    }

    [RelayCommand]
    private void CloseCreateOrderDialog()
    {
        IsCreateOrderDialogOpen = false;
        CurrentOrderItems.Clear();
    }

    [RelayCommand]
    private void AddProductToOrder()
    {
        if (SelectedProduct == null)
        {
            ShowError("请先选择一个商品");
            return;
        }

        var existingItem = CurrentOrderItems.FirstOrDefault(i => i.ProductId == SelectedProduct.Id);
        if (existingItem != null)
        {
            existingItem.Quantity++;
        }
        else
        {
            CurrentOrderItems.Add(new OrderItemViewModel(SelectedProduct));
        }
    }

    [RelayCommand]
    private void RemoveItemFromOrder(OrderItemViewModel? item)
    {
        if (item != null)
        {
            CurrentOrderItems.Remove(item);
        }
    }

    [RelayCommand]
    private async Task CreateOrderAsync()
    {
        if (SelectedCustomer == null)
        {
            ShowError("请选择客户");
            return;
        }

        if (CurrentOrderItems.Count == 0)
        {
            ShowError("请添加至少一个商品");
            return;
        }

        try
        {
            var items = CurrentOrderItems.Select(i => (i.ProductId, i.Quantity)).ToList();
            await _orderService.CreateOrderAsync(SelectedCustomer.Id, items, OrderNotes);

            CloseCreateOrderDialog();
            await LoadDataAsync();
            MessageBox.Show("订单创建成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    [RelayCommand]
    private async Task ViewOrderDetailAsync()
    {
        if (SelectedOrder == null) return;

        try
        {
            SelectedOrderDetail = await _orderService.GetOrderDetailByIdAsync(SelectedOrder.Id);
            IsOrderDetailDialogOpen = true;
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    [RelayCommand]
    private void CloseOrderDetailDialog()
    {
        IsOrderDetailDialogOpen = false;
        SelectedOrderDetail = null;
    }

    [RelayCommand]
    private async Task UpdateOrderStatusAsync(OrderStatus newStatus)
    {
        if (SelectedOrder == null) return;

        try
        {
            await _orderService.UpdateOrderStatusAsync(SelectedOrder.Id, newStatus);
            await LoadDataAsync();

            if (SelectedOrderDetail != null && SelectedOrderDetail.Order.Id == SelectedOrder.Id)
            {
                SelectedOrderDetail = await _orderService.GetOrderDetailByIdAsync(SelectedOrder.Id);
            }

            MessageBox.Show($"订单状态已更新为 {GetStatusDisplayName(newStatus)}", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    [RelayCommand]
    private async Task DeleteOrderAsync()
    {
        if (SelectedOrder == null) return;

        var result = MessageBox.Show("确定要删除此订单吗？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _orderService.DeleteOrderAsync(SelectedOrder.Id);
            await LoadDataAsync();
            MessageBox.Show("订单已删除", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    public string GetStatusDisplayName(string status)
    {
        if (Enum.TryParse<OrderStatus>(status, out var orderStatus))
        {
            return GetStatusDisplayName(orderStatus);
        }
        return status;
    }

    public string GetStatusDisplayName(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => "待处理",
            OrderStatus.Shipped => "已发货",
            OrderStatus.Completed => "已完成",
            OrderStatus.Cancelled => "已取消",
            _ => status.ToString()
        };
    }

    public async Task<List<OrderStatus>> GetAvailableStatusTransitionsAsync()
    {
        if (SelectedOrder == null) return new List<OrderStatus>();
        return await _orderService.GetAvailableStatusTransitionsAsync(SelectedOrder.Id);
    }

    public decimal GetOrderTotal()
    {
        return CurrentOrderItems.Sum(i => i.Subtotal);
    }

    private void ShowError(string message)
    {
        ErrorMessage = message;
        ShowErrorMessage = true;
        Task.Delay(3000).ContinueWith(_ =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ShowErrorMessage = false;
            });
        });
    }
}
