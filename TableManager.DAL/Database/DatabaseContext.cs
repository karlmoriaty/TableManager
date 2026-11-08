using Microsoft.Data.Sqlite;

namespace TableManager.DAL.Database;

public class DatabaseContext
{
    private readonly string _dbPath;

    public DatabaseContext(string dbPath = "tablemanager.db")
    {
        _dbPath = dbPath;
    }

    public SqliteConnection GetConnection()
    {
        return new SqliteConnection($"Data Source={_dbPath}");
    }

    public async Task InitializeDatabaseAsync()
    {
        using var connection = GetConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Products (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Code TEXT,
                Category TEXT,
                Price REAL NOT NULL,
                StockQuantity INTEGER NOT NULL DEFAULT 0,
                Description TEXT,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Customers (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Phone TEXT,
                Email TEXT,
                Address TEXT,
                Company TEXT,
                Notes TEXT,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Contacts (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CustomerId INTEGER NOT NULL,
                Name TEXT NOT NULL,
                Phone TEXT,
                Email TEXT,
                Position TEXT,
                IsPrimary INTEGER NOT NULL DEFAULT 0,
                Notes TEXT,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
            );

            CREATE TABLE IF NOT EXISTS Orders (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                OrderNumber TEXT NOT NULL,
                CustomerId INTEGER NOT NULL,
                TotalAmount REAL NOT NULL,
                Status TEXT NOT NULL,
                OrderDate TEXT NOT NULL,
                Notes TEXT,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
            );

            CREATE TABLE IF NOT EXISTS OrderItems (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                OrderId INTEGER NOT NULL,
                ProductId INTEGER NOT NULL,
                Quantity INTEGER NOT NULL,
                UnitPrice REAL NOT NULL,
                Subtotal REAL NOT NULL,
                CreatedAt TEXT NOT NULL,
                FOREIGN KEY (OrderId) REFERENCES Orders(Id),
                FOREIGN KEY (ProductId) REFERENCES Products(Id)
            );

            CREATE TABLE IF NOT EXISTS StockRecords (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ProductId INTEGER NOT NULL,
                Type TEXT NOT NULL,
                Quantity INTEGER NOT NULL,
                BeforeStock INTEGER NOT NULL,
                AfterStock INTEGER NOT NULL,
                Notes TEXT,
                CreatedAt TEXT NOT NULL,
                FOREIGN KEY (ProductId) REFERENCES Products(Id)
            );
        ";

        await command.ExecuteNonQueryAsync();
    }

    public async Task<SqliteTransaction> BeginTransactionAsync()
    {
        var connection = GetConnection();
        await connection.OpenAsync();
        return connection.BeginTransaction();
    }

    public async Task CommitTransactionAsync(SqliteTransaction transaction)
    {
        if (transaction.Connection != null)
        {
            await transaction.CommitAsync();
            await transaction.Connection.CloseAsync();
        }
    }

    public async Task RollbackTransactionAsync(SqliteTransaction transaction)
    {
        if (transaction.Connection != null)
        {
            await transaction.RollbackAsync();
            await transaction.Connection.CloseAsync();
        }
    }
}
