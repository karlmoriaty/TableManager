using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TableManager.BLL.Services;
using TableManager.DAL.Models;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TableManager.UI.ViewModels;

public partial class CustomersViewModel : ObservableObject, INotifyDataErrorInfo
{
    private readonly CustomerService _customerService;
    private readonly Dictionary<string, List<string>> _errors = new();

    [ObservableProperty]
    private ObservableCollection<Customer> _customers = new();

    [ObservableProperty]
    private ObservableCollection<Contact> _currentContacts = new();

    [ObservableProperty]
    private Customer? _selectedCustomer;

    [ObservableProperty]
    private Customer? _editingCustomer;

    [ObservableProperty]
    private Contact? _editingContact;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private bool _isEditDialogOpen = false;

    [ObservableProperty]
    private bool _isContactEditDialogOpen = false;

    [ObservableProperty]
    private string _editDialogTitle = "添加客户";

    [ObservableProperty]
    private string _contactEditDialogTitle = "添加联系人";

    [ObservableProperty]
    private string _searchTerm = string.Empty;

    public bool HasErrors => _errors.Count > 0;

    public Dictionary<string, List<string>> Errors => _errors;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public CustomersViewModel()
    {
        _customerService = new CustomerService();
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

    private void ValidateCustomer()
    {
        if (EditingCustomer == null) return;

        ClearErrors(nameof(EditingCustomer.Name));

        if (string.IsNullOrWhiteSpace(EditingCustomer.Name))
            AddError(nameof(EditingCustomer.Name), "客户名称不能为空");
    }

    private void ValidateContact()
    {
        if (EditingContact == null) return;

        ClearErrors(nameof(EditingContact.Name));

        if (string.IsNullOrWhiteSpace(EditingContact.Name))
            AddError(nameof(EditingContact.Name), "联系人名称不能为空");
    }

    [RelayCommand]
    private async Task LoadCustomersAsync()
    {
        IsLoading = true;
        try
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                var customers = await _customerService.GetAllCustomersAsync();
                Customers = new ObservableCollection<Customer>(customers);
            }
            else
            {
                var customers = await _customerService.SearchCustomersAsync(SearchTerm);
                Customers = new ObservableCollection<Customer>(customers);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadCustomersAsync();
    }

    [RelayCommand]
    private void AddCustomer()
    {
        EditingCustomer = new Customer();
        EditDialogTitle = "添加客户";
        IsEditDialogOpen = true;
        _errors.Clear();
    }

    [RelayCommand]
    private void EditCustomer()
    {
        if (SelectedCustomer != null)
        {
            EditingCustomer = new Customer
            {
                Id = SelectedCustomer.Id,
                Name = SelectedCustomer.Name,
                Phone = SelectedCustomer.Phone,
                Email = SelectedCustomer.Email,
                Address = SelectedCustomer.Address,
                Company = SelectedCustomer.Company,
                Notes = SelectedCustomer.Notes,
                CreatedAt = SelectedCustomer.CreatedAt,
                UpdatedAt = SelectedCustomer.UpdatedAt,
                Contacts = new List<Contact>(SelectedCustomer.Contacts)
            };
            EditDialogTitle = "编辑客户";
            IsEditDialogOpen = true;
            _errors.Clear();
        }
    }

    [RelayCommand]
    private async Task SaveCustomerAsync()
    {
        if (EditingCustomer == null) return;

        ValidateCustomer();
        if (HasErrors) return;

        try
        {
            if (EditingCustomer.Id == 0)
            {
                await _customerService.AddCustomerAsync(EditingCustomer);
            }
            else
            {
                await _customerService.UpdateCustomerAsync(EditingCustomer);
            }
            await LoadCustomersAsync();
            IsEditDialogOpen = false;
            EditingCustomer = null;
            _errors.Clear();
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
        EditingCustomer = null;
        _errors.Clear();
    }

    [RelayCommand]
    private async Task DeleteCustomerAsync()
    {
        if (SelectedCustomer == null) return;

        var result = System.Windows.MessageBox.Show(
            $"确定要删除客户 \"{SelectedCustomer.Name}\" 吗？这将同时删除该客户的所有联系人。",
            "确认删除",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
            try
            {
                await _customerService.DeleteCustomerAsync(SelectedCustomer.Id);
                await LoadCustomersAsync();
                SelectedCustomer = null;
                CurrentContacts.Clear();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"删除失败: {ex.Message}", "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private void LoadContacts()
    {
        if (SelectedCustomer != null)
        {
            CurrentContacts = new ObservableCollection<Contact>(SelectedCustomer.Contacts);
        }
        else
        {
            CurrentContacts.Clear();
        }
    }

    [RelayCommand]
    private void AddContact()
    {
        if (SelectedCustomer == null)
        {
            System.Windows.MessageBox.Show("请先选择一个客户", "提示", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            return;
        }

        EditingContact = new Contact { CustomerId = SelectedCustomer.Id };
        ContactEditDialogTitle = "添加联系人";
        IsContactEditDialogOpen = true;
        _errors.Clear();
    }

    [RelayCommand]
    private void EditContact()
    {
        if (SelectedCustomer == null || !CurrentContacts.Any()) return;

        var selectedContact = CurrentContacts.FirstOrDefault();
        if (selectedContact != null)
        {
            EditingContact = new Contact
            {
                Id = selectedContact.Id,
                CustomerId = selectedContact.CustomerId,
                Name = selectedContact.Name,
                Phone = selectedContact.Phone,
                Email = selectedContact.Email,
                Position = selectedContact.Position,
                IsPrimary = selectedContact.IsPrimary,
                Notes = selectedContact.Notes,
                CreatedAt = selectedContact.CreatedAt,
                UpdatedAt = selectedContact.UpdatedAt
            };
            ContactEditDialogTitle = "编辑联系人";
            IsContactEditDialogOpen = true;
            _errors.Clear();
        }
    }

    [RelayCommand]
    private async Task SaveContactAsync()
    {
        if (EditingContact == null) return;

        ValidateContact();
        if (HasErrors) return;

        try
        {
            if (EditingContact.Id == 0)
            {
                await _customerService.AddContactAsync(EditingContact);
            }
            else
            {
                await _customerService.UpdateContactAsync(EditingContact);
            }

            await LoadCustomersAsync();
            LoadContacts();
            IsContactEditDialogOpen = false;
            EditingContact = null;
            _errors.Clear();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"保存联系人失败: {ex.Message}", "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void CancelContactEdit()
    {
        IsContactEditDialogOpen = false;
        EditingContact = null;
        _errors.Clear();
    }

    [RelayCommand]
    private async Task DeleteContactAsync()
    {
        if (SelectedCustomer == null || !CurrentContacts.Any()) return;

        var selectedContact = CurrentContacts.FirstOrDefault();
        if (selectedContact == null) return;

        var result = System.Windows.MessageBox.Show(
            $"确定要删除联系人 \"{selectedContact.Name}\" 吗？",
            "确认删除",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
            try
            {
                await _customerService.DeleteContactAsync(selectedContact.Id);
                await LoadCustomersAsync();
                LoadContacts();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"删除联系人失败: {ex.Message}", "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
