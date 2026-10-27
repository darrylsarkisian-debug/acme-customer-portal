using Microsoft.Data.Sqlite;

namespace AcmePortal.Web.Data;

public class Database
{
    private readonly string _connectionString;

    public Database(string connectionString)
    {
        _connectionString = connectionString;
    }

    public SqliteConnection Open()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    public void Initialize()
    {
        using var connection = Open();

        Execute(connection, @"
            CREATE TABLE IF NOT EXISTS customers (
                id INTEGER PRIMARY KEY,
                name TEXT NOT NULL,
                email TEXT NOT NULL UNIQUE
            );");

        Execute(connection, @"
            CREATE TABLE IF NOT EXISTS orders (
                id INTEGER PRIMARY KEY,
                customer_id INTEGER NOT NULL,
                description TEXT NOT NULL,
                total REAL NOT NULL,
                status TEXT NOT NULL,
                placed_on TEXT NOT NULL,
                FOREIGN KEY (customer_id) REFERENCES customers(id)
            );");

        using var count = connection.CreateCommand();
        count.CommandText = "SELECT COUNT(*) FROM customers";
        if (Convert.ToInt64(count.ExecuteScalar()) == 0)
        {
            Seed(connection);
        }
    }

    private static void Seed(SqliteConnection connection)
    {
        AddCustomer(connection, 1, "Dana Whitfield", "dana@acme.example");
        AddCustomer(connection, 2, "Marcus Lee", "marcus@acme.example");

        AddOrder(connection, 1001, 1, "Industrial label printer", 1249.00, "Delivered", "2026-08-14");
        AddOrder(connection, 1002, 1, "Thermal label rolls (12 pack)", 86.50, "Delivered", "2026-09-02");
        AddOrder(connection, 1003, 1, "Barcode scanner", 219.99, "Shipped", "2026-10-11");
        AddOrder(connection, 1004, 2, "Warehouse shelving unit", 640.00, "Processing", "2026-10-20");
        AddOrder(connection, 1005, 2, "Pallet jack", 415.25, "Delivered", "2026-09-27");
    }

    private static void AddCustomer(SqliteConnection connection, long id, string name, string email)
    {
        Execute(connection,
            "INSERT INTO customers (id, name, email) VALUES (@id, @name, @email)",
            ("@id", id), ("@name", name), ("@email", email));
    }

    private static void AddOrder(SqliteConnection connection, long id, long customerId,
        string description, double total, string status, string placedOn)
    {
        Execute(connection,
            "INSERT INTO orders (id, customer_id, description, total, status, placed_on) " +
            "VALUES (@id, @customerId, @description, @total, @status, @placedOn)",
            ("@id", id), ("@customerId", customerId), ("@description", description),
            ("@total", total), ("@status", status), ("@placedOn", placedOn));
    }

    private static void Execute(SqliteConnection connection, string sql,
        params (string Name, object Value)[] parameters)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }
        command.ExecuteNonQuery();
    }
}
