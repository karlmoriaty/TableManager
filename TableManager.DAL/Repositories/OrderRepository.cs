using Microsoft.Data.Sqlite;
using TableManager.DAL.Database;
using TableManager.DAL.Models;

namespace TableManager.DAL.Repositories;

public class OrderRepository : IRepository<Order>
{
    private readonly DatabaseContext _dbContext;

    public OrderRepository(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        var orders = new List<Order>();
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Orders";

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            orders.Add(MapToOrder(reader));
        }

        return orders;
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Orders WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapToOrder(reader);
        }

        return null;
    }

    public async Task AddAsync(Order entity)
    {
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Orders (OrderNumber, CustomerId, TotalAmount, Status, OrderDate, Notes, CreatedAt, UpdatedAt)
            VALUES (@orderNumber, @customerId, @totalAmount, @status, @orderDate, @notes, @createdAt, @updatedAt);
            SELECT last_insert_rowid();
        ";

        AddOrderParameters(command, entity);

        var result = await command.ExecuteScalarAsync();
        if (result != null)
        {
            entity.Id = Convert.ToInt32(result);
        }
    }

    public async Task UpdateAsync(Order entity)
    {
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Orders
            SET OrderNumber = @orderNumber, CustomerId = @customerId, TotalAmount = @totalAmount,
                Status = @status, OrderDate = @orderDate, Notes = @notes, UpdatedAt = @updatedAt
            WHERE Id = @id
        ";

        command.Parameters.AddWithValue("@id", entity.Id);
        AddOrderParameters(command, entity);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Orders WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id);

        await command.ExecuteNonQueryAsync();
    }

    public async Task AddAsync(Order entity, SqliteTransaction transaction)
    {
        var command = transaction.Connection!.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
            INSERT INTO Orders (OrderNumber, CustomerId, TotalAmount, Status, OrderDate, Notes, CreatedAt, UpdatedAt)
            VALUES (@orderNumber, @customerId, @totalAmount, @status, @orderDate, @notes, @createdAt, @updatedAt);
            SELECT last_insert_rowid();
        ";

        AddOrderParameters(command, entity);

        var result = await command.ExecuteScalarAsync();
        if (result != null)
        {
            entity.Id = Convert.ToInt32(result);
        }
    }

    public async Task UpdateAsync(Order entity, SqliteTransaction transaction)
    {
        var command = transaction.Connection!.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
            UPDATE Orders
            SET OrderNumber = @orderNumber, CustomerId = @customerId, TotalAmount = @totalAmount,
                Status = @status, OrderDate = @orderDate, Notes = @notes, UpdatedAt = @updatedAt
            WHERE Id = @id
        ";

        command.Parameters.AddWithValue("@id", entity.Id);
        AddOrderParameters(command, entity);

        await command.ExecuteNonQueryAsync();
    }

    private Order MapToOrder(SqliteDataReader reader)
    {
        return new Order
        {
            Id = reader.GetInt32(0),
            OrderNumber = reader.GetString(1),
            CustomerId = reader.GetInt32(2),
            TotalAmount = reader.GetDecimal(3),
            Status = reader.GetString(4),
            OrderDate = DateTime.Parse(reader.GetString(5)),
            Notes = reader.IsDBNull(6) ? null : reader.GetString(6),
            CreatedAt = DateTime.Parse(reader.GetString(7)),
            UpdatedAt = DateTime.Parse(reader.GetString(8))
        };
    }

    private void AddOrderParameters(SqliteCommand command, Order entity)
    {
        command.Parameters.AddWithValue("@orderNumber", entity.OrderNumber);
        command.Parameters.AddWithValue("@customerId", entity.CustomerId);
        command.Parameters.AddWithValue("@totalAmount", entity.TotalAmount);
        command.Parameters.AddWithValue("@status", entity.Status);
        command.Parameters.AddWithValue("@orderDate", entity.OrderDate.ToString("o"));
        command.Parameters.AddWithValue("@notes", entity.Notes ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@createdAt", entity.CreatedAt.ToString("o"));
        command.Parameters.AddWithValue("@updatedAt", entity.UpdatedAt.ToString("o"));
    }
}
