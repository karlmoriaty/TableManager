using CommunityToolkit.Mvvm.ComponentModel;
using TableManager.DAL.Models;

namespace TableManager.UI.ViewModels;

public partial class OrderItemViewModel : ObservableObject
{
    [ObservableProperty]
    private int _productId;

    [ObservableProperty]
    private string _productName = string.Empty;

    [ObservableProperty]
    private string? _productCode;

    [ObservableProperty]
    private decimal _unitPrice;

    [ObservableProperty]
    private int _quantity = 1;

    [ObservableProperty]
    private decimal _subtotal;

    public OrderItemViewModel()
    {
    }

    public OrderItemViewModel(Product product)
    {
        ProductId = product.Id;
        ProductName = product.Name;
        ProductCode = product.Code;
        UnitPrice = product.Price;
        Subtotal = UnitPrice * Quantity;
    }

    partial void OnQuantityChanged(int value)
    {
        Subtotal = UnitPrice * value;
    }

    partial void OnUnitPriceChanged(decimal value)
    {
        Subtotal = value * Quantity;
    }
}
