using Microsoft.Data.Sqlite;
using TableManager.DAL.Database;
using TableManager.DAL.Models;

namespace TableManager.DAL.Repositories;

public class StockRecordRepository : IRepository<StockRecord>
{
    private readonly DatabaseContext _dbContext;

    public StockRecordRepository(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<StockRecord>> GetAllAsync()
    {
        var stockRecords = new List<StockRecord>();
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM StockRecords";

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            stockRecords.Add(MapToStockRecord(reader));
        }

        return stockRecords;
    }

    public async Task<IEnumerable<StockRecord>> GetByProductIdAsync(int productId)
    {
        var stockRecords = new List<StockRecord>();
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM StockRecords WHERE ProductId = @productId";
        command.Parameters.AddWithValue("@productId", productId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            stockRecords.Add(MapToStockRecord(reader));
        }

        return stockRecords;
    }

    public async Task<StockRecord?> GetByIdAsync(int id)
    {
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM StockRecords WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapToStockRecord(reader);
        }

        return null;
    }

    public async Task AddAsync(StockRecord entity)
    {
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO StockRecords (ProductId, Type, Quantity, BeforeStock, AfterStock, Notes, CreatedAt)
            VALUES (@productId, @type, @quantity, @beforeStock, @afterStock, @notes, @createdAt);
            SELECT last_insert_rowid();
        ";

        AddStockRecordParameters(command, entity);

        var result = await command.ExecuteScalarAsync();
        if (result != null)
        {
            entity.Id = Convert.ToInt32(result);
        }
    }

    public async Task UpdateAsync(StockRecord entity)
    {
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE StockRecords
            SET ProductId = @productId, Type = @type, Quantity = @quantity,
                BeforeStock = @beforeStock, AfterStock = @afterStock, Notes = @notes
            WHERE Id = @id
        ";

        command.Parameters.AddWithValue("@id", entity.Id);
        AddStockRecordParameters(command, entity);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM StockRecords WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id);

        await command.ExecuteNonQueryAsync();
    }

    public async Task AddAsync(StockRecord entity, SqliteTransaction transaction)
    {
        var command = transaction.Connection!.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
            INSERT INTO StockRecords (ProductId, Type, Quantity, BeforeStock, AfterStock, Notes, CreatedAt)
            VALUES (@productId, @type, @quantity, @beforeStock, @afterStock, @notes, @createdAt);
            SELECT last_insert_rowid();
        ";

        AddStockRecordParameters(command, entity);

        var result = await command.ExecuteScalarAsync();
        if (result != null)
        {
            entity.Id = Convert.ToInt32(result);
        }
    }

    public async Task UpdateAsync(StockRecord entity, SqliteTransaction transaction)
    {
        var command = transaction.Connection!.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
            UPDATE StockRecords
            SET ProductId = @productId, Type = @type, Quantity = @quantity,
                BeforeStock = @beforeStock, AfterStock = @afterStock, Notes = @notes
            WHERE Id = @id
        ";

        command.Parameters.AddWithValue("@id", entity.Id);
        AddStockRecordParameters(command, entity);

        await command.ExecuteNonQueryAsync();
    }

    private StockRecord MapToStockRecord(SqliteDataReader reader)
    {
        return new StockRecord
        {
            Id = reader.GetInt32(0),
            ProductId = reader.GetInt32(1),
            Type = reader.GetString(2),
            Quantity = reader.GetInt32(3),
            BeforeStock = reader.GetInt32(4),
            AfterStock = reader.GetInt32(5),
            Notes = reader.IsDBNull(6) ? null : reader.GetString(6),
            CreatedAt = DateTime.Parse(reader.GetString(7))
        };
    }

    private void AddStockRecordParameters(SqliteCommand command, StockRecord entity)
    {
        command.Parameters.AddWithValue("@productId", entity.ProductId);
        command.Parameters.AddWithValue("@type", entity.Type);
        command.Parameters.AddWithValue("@quantity", entity.Quantity);
        command.Parameters.AddWithValue("@beforeStock", entity.BeforeStock);
        command.Parameters.AddWithValue("@afterStock", entity.AfterStock);
        command.Parameters.AddWithValue("@notes", entity.Notes ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@createdAt", entity.CreatedAt.ToString("o"));
    }
}
