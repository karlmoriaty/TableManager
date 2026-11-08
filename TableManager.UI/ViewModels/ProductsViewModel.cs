using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TableManager.BLL.Services;
using TableManager.DAL.Models;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TableManager.UI.ViewModels;

public partial class ProductsViewModel : ObservableObject, INotifyDataErrorInfo
{
    private readonly ProductService _productService;
    private readonly Dictionary<string, List<string>> _errors = new();

    [ObservableProperty]
    private ObservableCollection<Product> _products = new();

    [ObservableProperty]
    private Product? _selectedProduct;

    [ObservableProperty]
    private Product? _editingProduct;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private bool _isEditDialogOpen = false;

    [ObservableProperty]
    private string _editDialogTitle = "添加商品";

    public bool HasErrors => _errors.Count > 0;

    public Dictionary<string, List<string>> Errors => _errors;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public ProductsViewModel()
    {
        _productService = new ProductService();
    }

    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return _errors.Values.SelectMany(x => x);
        return _errors.TryGetValue(propertyName, out var errors) ? errors : Enumerable.Empty<string>();
    }

    private void AddError(string propertyName, string error)
    {
        if (!_errors.ContainsKey(propertyName))
            _errors[propertyName] = new List<string>();
        if (!_errors[propertyName].Contains(error))
        {
            _errors[propertyName].Add(error);
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }

    private void ClearErrors(string propertyName)
    {
        if (_errors.Remove(propertyName))
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    private void ValidateProduct()
    {
        if (EditingProduct == null) return;

        ClearErrors(nameof(EditingProduct.Name));
        ClearErrors(nameof(EditingProduct.Price));
        ClearErrors(nameof(EditingProduct.StockQuantity));

        if (string.IsNullOrWhiteSpace(EditingProduct.Name))
            AddError(nameof(EditingProduct.Name), "商品名称不能为空");

        if (EditingProduct.Price < 0)
            AddError(nameof(EditingProduct.Price), "价格不能为负数");

        if (EditingProduct.StockQuantity < 0)
            AddError(nameof(EditingProduct.StockQuantity), "库存数量不能为负数");
    }

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        IsLoading = true;
        try
        {
            var products = await _productService.GetAllProductsAsync();
            Products = new ObservableCollection<Product>(products);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void AddProduct()
    {
        EditingProduct = new Product();
        EditDialogTitle = "添加商品";
        IsEditDialogOpen = true;
    }

    [RelayCommand]
    private void EditProduct()
    {
        if (SelectedProduct != null)
        {
            EditingProduct = new Product
            {
                Id = SelectedProduct.Id,
                Name = SelectedProduct.Name,
                Code = SelectedProduct.Code,
                Category = SelectedProduct.Category,
                Price = SelectedProduct.Price,
                StockQuantity = SelectedProduct.StockQuantity,
                Description = SelectedProduct.Description,
                CreatedAt = SelectedProduct.CreatedAt,
                UpdatedAt = SelectedProduct.UpdatedAt
            };
            EditDialogTitle = "编辑商品";
            IsEditDialogOpen = true;
        }
    }

    [RelayCommand]
    private async Task SaveProductAsync()
    {
        if (EditingProduct == null) return;

        ValidateProduct();
        if (HasErrors) return;

        try
        {
            if (EditingProduct.Id == 0)
            {
                await _productService.AddProductAsync(EditingProduct);
            }
            else
            {
                await _productService.UpdateProductAsync(EditingProduct);
            }
            await LoadProductsAsync();
            IsEditDialogOpen = false;
            EditingProduct = null;
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"保存失败: {ex.Message}", "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditDialogOpen = false;
        EditingProduct = null;
        _errors.Clear();
    }

    [RelayCommand]
    private async Task DeleteProductAsync()
    {
        if (SelectedProduct == null) return;

        var result = System.Windows.MessageBox.Show(
            $"确定要删除商品 \"{SelectedProduct.Name}\"吗？",
            "确认删除",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
            try
            {
                await _productService.DeleteProductAsync(SelectedProduct.Id);
                await LoadProductsAsync();
                SelectedProduct = null;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"删除失败: {ex.Message}", "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
