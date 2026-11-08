using TableManager.DAL.Database;
using TableManager.DAL.Models;
using TableManager.DAL.Repositories;

namespace TableManager.BLL.Services;

public class CustomerService
{
    private readonly CustomerRepository _customerRepository;

    public CustomerService()
    {
        var dbContext = DatabaseManager.GetSharedContext();
        _customerRepository = new CustomerRepository(dbContext);
    }

    public CustomerService(DatabaseContext dbContext)
    {
        _customerRepository = new CustomerRepository(dbContext);
    }

    public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
    {
        return await _customerRepository.GetAllAsync();
    }

    public async Task<Customer?> GetCustomerByIdAsync(int id)
    {
        return await _customerRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm)
    {
        // 暂时简化实现，返回所有客户
        return await _customerRepository.GetAllAsync();
    }

    public async Task AddCustomerAsync(Customer customer)
    {
        customer.CreatedAt = DateTime.UtcNow;
        customer.UpdatedAt = DateTime.UtcNow;
        await _customerRepository.AddAsync(customer);
    }

    public async Task UpdateCustomerAsync(Customer customer)
    {
        customer.UpdatedAt = DateTime.UtcNow;
        await _customerRepository.UpdateAsync(customer);
    }

    public async Task DeleteCustomerAsync(int id)
    {
        await _customerRepository.DeleteAsync(id);
    }

    // 虚拟方法，暂时避免编译错误
    public async Task<IEnumerable<object>> GetContactsByCustomerIdAsync(int customerId)
    {
        await Task.CompletedTask;
        return new List<object>();
    }

    public async Task AddContactAsync(object contact)
    {
        await Task.CompletedTask;
    }

    public async Task UpdateContactAsync(object contact)
    {
        await Task.CompletedTask;
    }

    public async Task DeleteContactAsync(int id)
    {
        await Task.CompletedTask;
    }
}
