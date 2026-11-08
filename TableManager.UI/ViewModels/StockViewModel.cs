using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TableManager.BLL.Services;
using TableManager.DAL.Models;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TableManager.UI.ViewModels;

public partial class StockViewModel : ObservableObject, INotifyDataErrorInfo
{
    private readonly StockService _stockService;
    private readonly ProductService _productService;
    private readonly Dictionary<string, List<string>> _errors = new();

    [ObservableProperty]
    private ObservableCollection<Product> _products = new();

    [ObservableProperty]
    private ObservableCollection<StockRecord> _stockRecords = new();

    [ObservableProperty]
    private Product? _selectedProduct;

    [ObservableProperty]
    private string _operationType = "In";

    [ObservableProperty]
    private int _quantity = 1;

    [ObservableProperty]
    private string? _notes;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private bool _isProcessing = false;

    public bool HasErrors => _errors.Count > 0;

    public Dictionary<string, List<string>> Errors => _errors;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public StockViewModel()
    {
        _stockService = new StockService();
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

    private void ValidateOperation()
    {
        ClearErrors(nameof(SelectedProduct));
        ClearErrors(nameof(Quantity));

        if (SelectedProduct == null)
            AddError(nameof(SelectedProduct), "请选择商品");

        if (Quantity <= 0)
            AddError(nameof(Quantity), "数量必须大于0");

        if (SelectedProduct != null && OperationType == "Out" && SelectedProduct.StockQuantity < Quantity)
            AddError(nameof(Quantity), $"库存不足！当前库存: {SelectedProduct.StockQuantity}");
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var products = await _productService.GetAllProductsAsync();
            Products = new ObservableCollection<Product>(products);

            var stockRecords = await _stockService.GetAllStockRecordsAsync();
            StockRecords = new ObservableCollection<StockRecord>(stockRecords.OrderByDescending(s => s.CreatedAt));
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ExecuteOperationAsync()
    {
        ValidateOperation();
        if (HasErrors) return;

        if (SelectedProduct == null) return;

        IsProcessing = true;
        try
        {
            if (OperationType == "In")
            {
                await _stockService.StockInAsync(SelectedProduct.Id, Quantity, Notes);
            }
            else
            {
                await _stockService.StockOutAsync(SelectedProduct.Id, Quantity, Notes);
            }

            await LoadDataAsync();
            Quantity = 1;
            Notes = string.Empty;
            _errors.Clear();
            System.Windows.MessageBox.Show("操作成功！", "提示", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
        catch (InvalidOperationException ex)
        {
            System.Windows.MessageBox.Show(ex.Message, "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"操作失败: {ex.Message}", "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
