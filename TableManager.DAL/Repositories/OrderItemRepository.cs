using Microsoft.Data.Sqlite;
using TableManager.DAL.Database;
using TableManager.DAL.Models;

namespace TableManager.DAL.Repositories;

public class OrderItemRepository : IRepository<OrderItem>
{
    private readonly DatabaseContext _dbContext;

    public OrderItemRepository(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<OrderItem>> GetAllAsync()
    {
        var orderItems = new List<OrderItem>();
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM OrderItems";

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            orderItems.Add(MapToOrderItem(reader));
        }

        return orderItems;
    }

    public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId)
    {
        var orderItems = new List<OrderItem>();
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM OrderItems WHERE OrderId = @orderId";
        command.Parameters.AddWithValue("@orderId", orderId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            orderItems.Add(MapToOrderItem(reader));
        }

        return orderItems;
    }

    public async Task<OrderItem?> GetByIdAsync(int id)
    {
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM OrderItems WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapToOrderItem(reader);
        }

        return null;
    }

    public async Task AddAsync(OrderItem entity)
    {
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, Subtotal, CreatedAt)
            VALUES (@orderId, @productId, @quantity, @unitPrice, @subtotal, @createdAt);
            SELECT last_insert_rowid();
        ";

        AddOrderItemParameters(command, entity);

        var result = await command.ExecuteScalarAsync();
        if (result != null)
        {
            entity.Id = Convert.ToInt32(result);
        }
    }

    public async Task UpdateAsync(OrderItem entity)
    {
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE OrderItems
            SET OrderId = @orderId, ProductId = @productId, Quantity = @quantity,
                UnitPrice = @unitPrice, Subtotal = @subtotal
            WHERE Id = @id
        ";

        command.Parameters.AddWithValue("@id", entity.Id);
        AddOrderItemParameters(command, entity);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM OrderItems WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id);

        await command.ExecuteNonQueryAsync();
    }

    public async Task AddAsync(OrderItem entity, SqliteTransaction transaction)
    {
        var command = transaction.Connection!.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
            INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, Subtotal, CreatedAt)
            VALUES (@orderId, @productId, @quantity, @unitPrice, @subtotal, @createdAt);
            SELECT last_insert_rowid();
        ";

        AddOrderItemParameters(command, entity);

        var result = await command.ExecuteScalarAsync();
        if (result != null)
        {
            entity.Id = Convert.ToInt32(result);
        }
    }

    public async Task UpdateAsync(OrderItem entity, SqliteTransaction transaction)
    {
        var command = transaction.Connection!.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
            UPDATE OrderItems
            SET OrderId = @orderId, ProductId = @productId, Quantity = @quantity,
                UnitPrice = @unitPrice, Subtotal = @subtotal
            WHERE Id = @id
        ";

        command.Parameters.AddWithValue("@id", entity.Id);
        AddOrderItemParameters(command, entity);

        await command.ExecuteNonQueryAsync();
    }

    private OrderItem MapToOrderItem(SqliteDataReader reader)
    {
        return new OrderItem
        {
            Id = reader.GetInt32(0),
            OrderId = reader.GetInt32(1),
            ProductId = reader.GetInt32(2),
            Quantity = reader.GetInt32(3),
            UnitPrice = reader.GetDecimal(4),
            Subtotal = reader.GetDecimal(5),
            CreatedAt = DateTime.Parse(reader.GetString(6))
        };
    }

    private void AddOrderItemParameters(SqliteCommand command, OrderItem entity)
    {
        command.Parameters.AddWithValue("@orderId", entity.OrderId);
        command.Parameters.AddWithValue("@productId", entity.ProductId);
        command.Parameters.AddWithValue("@quantity", entity.Quantity);
        command.Parameters.AddWithValue("@unitPrice", entity.UnitPrice);
        command.Parameters.AddWithValue("@subtotal", entity.Subtotal);
        command.Parameters.AddWithValue("@createdAt", entity.CreatedAt.ToString("o"));
    }
}
