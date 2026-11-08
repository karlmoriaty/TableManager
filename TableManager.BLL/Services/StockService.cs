using TableManager.DAL.Database;
using TableManager.DAL.Models;
using TableManager.DAL.Repositories;

namespace TableManager.BLL.Services;

public class StockService
{
    private readonly ProductRepository _productRepository;
    private readonly StockRecordRepository _stockRecordRepository;
    private readonly DatabaseContext _dbContext;

    public StockService()
    {
        _dbContext = DatabaseManager.GetSharedContext();
        _productRepository = new ProductRepository(_dbContext);
        _stockRecordRepository = new StockRecordRepository(_dbContext);
    }

    public StockService(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
        _productRepository = new ProductRepository(_dbContext);
        _stockRecordRepository = new StockRecordRepository(_dbContext);
    }

    public async Task<IEnumerable<StockRecord>> GetAllStockRecordsAsync()
    {
        return await _stockRecordRepository.GetAllAsync();
    }

    public async Task<IEnumerable<StockRecord>> GetStockRecordsByProductIdAsync(int productId)
    {
        return await _stockRecordRepository.GetByProductIdAsync(productId);
    }

    public async Task StockInAsync(int productId, int quantity, string? notes = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("入库数量必须大于0");

        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            throw new InvalidOperationException("商品不存在");

        var transaction = await _dbContext.BeginTransactionAsync();
        try
        {
            var beforeStock = product.StockQuantity;
            product.StockQuantity += quantity;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product, transaction);

            var stockRecord = new StockRecord
            {
                ProductId = productId,
                Type = "In",
                Quantity = quantity,
                BeforeStock = beforeStock,
                AfterStock = product.StockQuantity,
                Notes = notes,
                CreatedAt = DateTime.UtcNow
            };

            await _stockRecordRepository.AddAsync(stockRecord, transaction);

            await _dbContext.CommitTransactionAsync(transaction);
        }
        catch
        {
            await _dbContext.RollbackTransactionAsync(transaction);
            throw;
        }
    }

    public async Task StockOutAsync(int productId, int quantity, string? notes = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("出库数量必须大于0");

        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            throw new InvalidOperationException("商品不存在");

        if (product.StockQuantity < quantity)
            throw new InvalidOperationException($"库存不足！当前库存: {product.StockQuantity}, 需要: {quantity}");

        var transaction = await _dbContext.BeginTransactionAsync();
        try
        {
            var beforeStock = product.StockQuantity;
            product.StockQuantity -= quantity;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product, transaction);

            var stockRecord = new StockRecord
            {
                ProductId = productId,
                Type = "Out",
                Quantity = quantity,
                BeforeStock = beforeStock,
                AfterStock = product.StockQuantity,
                Notes = notes,
                CreatedAt = DateTime.UtcNow
            };

            await _stockRecordRepository.AddAsync(stockRecord, transaction);

            await _dbContext.CommitTransactionAsync(transaction);
        }
        catch
        {
            await _dbContext.RollbackTransactionAsync(transaction);
            throw;
        }
    }
}
