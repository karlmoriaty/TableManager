using Microsoft.Data.Sqlite;
using TableManager.DAL.Database;
using TableManager.DAL.Models;

namespace TableManager.DAL.Repositories;

public class CustomerRepository : IRepository<Customer>
{
    private readonly DatabaseContext _dbContext;
    private readonly ContactRepository _contactRepository;

    public CustomerRepository(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
        _contactRepository = new ContactRepository(dbContext);
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        var customers = new List<Customer>();
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Customers";

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            customers.Add(MapToCustomer(reader));
        }

        foreach (var customer in customers)
        {
            customer.Contacts = (await _contactRepository.GetByCustomerIdAsync(customer.Id)).ToList();
        }

        return customers;
    }

    public async Task<IEnumerable<Customer>> SearchAsync(string searchTerm)
    {
        var customers = new List<Customer>();
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM Customers 
            WHERE Name LIKE @searchTerm 
            OR Phone LIKE @searchTerm 
            OR Email LIKE @searchTerm 
            OR Company LIKE @searchTerm
        ";
        command.Parameters.AddWithValue("@searchTerm", $"%{searchTerm}%");

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            customers.Add(MapToCustomer(reader));
        }

        foreach (var customer in customers)
        {
            customer.Contacts = (await _contactRepository.GetByCustomerIdAsync(customer.Id)).ToList();
        }

        return customers;
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Customers WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var customer = MapToCustomer(reader);
            customer.Contacts = (await _contactRepository.GetByCustomerIdAsync(customer.Id)).ToList();
            return customer;
        }

        return null;
    }

    public async Task AddAsync(Customer entity)
    {
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Customers (Name, Phone, Email, Address, Company, Notes, CreatedAt, UpdatedAt)
            VALUES (@name, @phone, @email, @address, @company, @notes, @createdAt, @updatedAt);
            SELECT last_insert_rowid();
        ";

        AddCustomerParameters(command, entity);

        var result = await command.ExecuteScalarAsync();
        if (result != null)
        {
            entity.Id = Convert.ToInt32(result);
        }
    }

    public async Task UpdateAsync(Customer entity)
    {
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Customers
            SET Name = @name, Phone = @phone, Email = @email, Address = @address,
                Company = @company, Notes = @notes, UpdatedAt = @updatedAt
            WHERE Id = @id
        ";

        command.Parameters.AddWithValue("@id", entity.Id);
        AddCustomerParameters(command, entity);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await _contactRepository.DeleteByCustomerIdAsync(id);
        
        using var connection = _dbContext.GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Customers WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id);

        await command.ExecuteNonQueryAsync();
    }

    public async Task AddAsync(Customer entity, SqliteTransaction transaction)
    {
        var command = transaction.Connection!.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
            INSERT INTO Customers (Name, Phone, Email, Address, Company, Notes, CreatedAt, UpdatedAt)
            VALUES (@name, @phone, @email, @address, @company, @notes, @createdAt, @updatedAt);
            SELECT last_insert_rowid();
        ";

        AddCustomerParameters(command, entity);

        var result = await command.ExecuteScalarAsync();
        if (result != null)
        {
            entity.Id = Convert.ToInt32(result);
        }
    }

    public async Task UpdateAsync(Customer entity, SqliteTransaction transaction)
    {
        var command = transaction.Connection!.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
            UPDATE Customers
            SET Name = @name, Phone = @phone, Email = @email, Address = @address,
                Company = @company, Notes = @notes, UpdatedAt = @updatedAt
            WHERE Id = @id
        ";

        command.Parameters.AddWithValue("@id", entity.Id);
        AddCustomerParameters(command, entity);

        await command.ExecuteNonQueryAsync();
    }

    private Customer MapToCustomer(SqliteDataReader reader)
    {
        return new Customer
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Phone = reader.IsDBNull(2) ? null : reader.GetString(2),
            Email = reader.IsDBNull(3) ? null : reader.GetString(3),
            Address = reader.IsDBNull(4) ? null : reader.GetString(4),
            Company = reader.IsDBNull(5) ? null : reader.GetString(5),
            Notes = reader.IsDBNull(6) ? null : reader.GetString(6),
            CreatedAt = DateTime.Parse(reader.GetString(7)),
            UpdatedAt = DateTime.Parse(reader.GetString(8)),
            Contacts = new List<Contact>()
        };
    }

    private void AddCustomerParameters(SqliteCommand command, Customer entity)
    {
        command.Parameters.AddWithValue("@name", entity.Name);
        command.Parameters.AddWithValue("@phone", entity.Phone ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@email", entity.Email ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@address", entity.Address ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@company", entity.Company ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@notes", entity.Notes ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@createdAt", entity.CreatedAt.ToString("o"));
        command.Parameters.AddWithValue("@updatedAt", entity.UpdatedAt.ToString("o"));
    }
}
