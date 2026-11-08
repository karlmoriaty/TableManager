using TableManager.DAL.Database;
using TableManager.DAL.Models;
using TableManager.DAL.Repositories;

namespace TableManager.BLL.Services;

public class ProductService
{
    private readonly ProductRepository _productRepository;

    public ProductService()
    {
        var dbContext = DatabaseManager.GetSharedContext();
        _productRepository = new ProductRepository(dbContext);
    }

    public ProductService(DatabaseContext dbContext)
    {
        _productRepository = new ProductRepository(dbContext);
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _productRepository.GetAllAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _productRepository.GetByIdAsync(id);
    }

    public async Task AddProductAsync(Product product)
    {
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        await _productRepository.AddAsync(product);
    }

    public async Task UpdateProductAsync(Product product)
    {
        product.UpdatedAt = DateTime.UtcNow;
        await _productRepository.UpdateAsync(product);
    }

    public async Task DeleteProductAsync(int id)
    {
        await _productRepository.DeleteAsync(id);
    }
}
