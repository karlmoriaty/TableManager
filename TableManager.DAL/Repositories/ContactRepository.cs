using Microsoft.Data.Sqlite;
using TableManager.DAL.Database;
using TableManager.DAL.Models;

namespace TableManager.DAL.Repositories;

public class ContactRepository
{
    private readonly DatabaseContext _dbContext;

    public ContactRepository(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Contact>> GetByCustomerIdAsync(int customerId)
    {
        var contacts = new List<Contact>();
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Contacts WHERE CustomerId = @customerId";
        command.Parameters.AddWithValue("@customerId", customerId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            contacts.Add(MapToContact(reader));
        }

        return contacts;
    }

    public async Task<Contact?> GetByIdAsync(int id)
    {
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Contacts WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapToContact(reader);
        }

        return null;
    }

    public async Task AddAsync(Contact entity)
    {
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Contacts (CustomerId, Name, Phone, Email, Position, IsPrimary, Notes, CreatedAt, UpdatedAt)
            VALUES (@customerId, @name, @phone, @email, @position, @isPrimary, @notes, @createdAt, @updatedAt);
            SELECT last_insert_rowid();
        ";

        AddContactParameters(command, entity);

        var result = await command.ExecuteScalarAsync();
        if (result != null)
        {
            entity.Id = Convert.ToInt32(result);
        }
    }

    public async Task UpdateAsync(Contact entity)
    {
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Contacts
            SET CustomerId = @customerId, Name = @name, Phone = @phone, Email = @email,
                Position = @position, IsPrimary = @isPrimary, Notes = @notes, UpdatedAt = @updatedAt
            WHERE Id = @id
        ";

        command.Parameters.AddWithValue("@id", entity.Id);
        AddContactParameters(command, entity);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Contacts WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteByCustomerIdAsync(int customerId)
    {
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Contacts WHERE CustomerId = @customerId";
        command.Parameters.AddWithValue("@customerId", customerId);

        await command.ExecuteNonQueryAsync();
    }

    private Contact MapToContact(SqliteDataReader reader)
    {
        return new Contact
        {
            Id = reader.GetInt32(0),
            CustomerId = reader.GetInt32(1),
            Name = reader.GetString(2),
            Phone = reader.IsDBNull(3) ? null : reader.GetString(3),
            Email = reader.IsDBNull(4) ? null : reader.GetString(4),
            Position = reader.IsDBNull(5) ? null : reader.GetString(5),
            IsPrimary = reader.GetInt32(6) != 0,
            Notes = reader.IsDBNull(7) ? null : reader.GetString(7),
            CreatedAt = DateTime.Parse(reader.GetString(8)),
            UpdatedAt = DateTime.Parse(reader.GetString(9))
        };
    }

    private void AddContactParameters(SqliteCommand command, Contact entity)
    {
        command.Parameters.AddWithValue("@customerId", entity.CustomerId);
        command.Parameters.AddWithValue("@name", entity.Name);
        command.Parameters.AddWithValue("@phone", entity.Phone ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@email", entity.Email ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@position", entity.Position ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@isPrimary", entity.IsPrimary ? 1 : 0);
        command.Parameters.AddWithValue("@notes", entity.Notes ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@createdAt", entity.CreatedAt.ToString("o"));
        command.Parameters.AddWithValue("@updatedAt", entity.UpdatedAt.ToString("o"));
    }
}
