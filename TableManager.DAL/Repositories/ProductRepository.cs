using Microsoft.Data.Sqlite;
using TableManager.DAL.Database;
using TableManager.DAL.Models;

namespace TableManager.DAL.Repositories;

public class ProductRepository : IRepository<Product>
{
    private readonly DatabaseContext _dbContext;

    public ProductRepository(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        var products = new List<Product>();
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Products";

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            products.Add(MapToProduct(reader));
        }

        return products;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Products WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapToProduct(reader);
        }

        return null;
    }

    public async Task AddAsync(Product entity)
    {
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Products (Name, Code, Category, Price, StockQuantity, Description, CreatedAt, UpdatedAt)
            VALUES (@name, @code, @category, @price, @stockQuantity, @description, @createdAt, @updatedAt);
            SELECT last_insert_rowid();
        ";

        AddProductParameters(command, entity);

        var result = await command.ExecuteScalarAsync();
        if (result != null)
        {
            entity.Id = Convert.ToInt32(result);
        }
    }

    public async Task UpdateAsync(Product entity)
    {
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Products
            SET Name = @name, Code = @code, Category = @category, Price = @price,
                StockQuantity = @stockQuantity, Description = @description, UpdatedAt = @updatedAt
            WHERE Id = @id
        ";

        command.Parameters.AddWithValue("@id", entity.Id);
        AddProductParameters(command, entity);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Products WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id);

        await command.ExecuteNonQueryAsync();
    }

    public async Task AddAsync(Product entity, SqliteTransaction transaction)
    {
        var command = transaction.Connection!.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
            INSERT INTO Products (Name, Code, Category, Price, StockQuantity, Description, CreatedAt, UpdatedAt)
            VALUES (@name, @code, @category, @price, @stockQuantity, @description, @createdAt, @updatedAt);
            SELECT last_insert_rowid();
        ";

        AddProductParameters(command, entity);

        var result = await command.ExecuteScalarAsync();
        if (result != null)
        {
            entity.Id = Convert.ToInt32(result);
        }
    }

    public async Task UpdateAsync(Product entity, SqliteTransaction transaction)
    {
        var command = transaction.Connection!.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
            UPDATE Products
            SET Name = @name, Code = @code, Category = @category, Price = @price,
                StockQuantity = @stockQuantity, Description = @description, UpdatedAt = @updatedAt
            WHERE Id = @id
        ";

        command.Parameters.AddWithValue("@id", entity.Id);
        AddProductParameters(command, entity);

        await command.ExecuteNonQueryAsync();
    }

    private Product MapToProduct(SqliteDataReader reader)
    {
        return new Product
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Code = reader.IsDBNull(2) ? null : reader.GetString(2),
            Category = reader.IsDBNull(3) ? null : reader.GetString(3),
            Price = reader.GetDecimal(4),
            StockQuantity = reader.GetInt32(5),
            Description = reader.IsDBNull(6) ? null : reader.GetString(6),
            CreatedAt = DateTime.Parse(reader.GetString(7)),
            UpdatedAt = DateTime.Parse(reader.GetString(8))
        };
    }

    private void AddProductParameters(SqliteCommand command, Product entity)
    {
        command.Parameters.AddWithValue("@name", entity.Name);
        command.Parameters.AddWithValue("@code", entity.Code ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@category", entity.Category ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@price", entity.Price);
        command.Parameters.AddWithValue("@stockQuantity", entity.StockQuantity);
        command.Parameters.AddWithValue("@description", entity.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@createdAt", entity.CreatedAt.ToString("o"));
        command.Parameters.AddWithValue("@updatedAt", entity.UpdatedAt.ToString("o"));
    }
}
