using System.Globalization;
using AcmePortal.Web.Data;
using AcmePortal.Web.Models;
using Microsoft.Data.Sqlite;

namespace AcmePortal.Web.Services;

public class OrderService
{
    private readonly Database _db;

    public OrderService(Database db)
    {
        _db = db;
    }

    public Customer? FindCustomerByEmail(string email)
    {
        using var connection = _db.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, name, email FROM customers WHERE email = @email";
        command.Parameters.AddWithValue("@email", email);

        using var reader = command.ExecuteReader();
        return reader.Read()
            ? new Customer(reader.GetInt64(0), reader.GetString(1), reader.GetString(2))
            : null;
    }

    public IReadOnlyList<Order> GetOrdersForCustomer(long customerId)
    {
        using var connection = _db.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT id, customer_id, description, total, status, placed_on " +
            "FROM orders WHERE customer_id = @customerId ORDER BY placed_on DESC";
        command.Parameters.AddWithValue("@customerId", customerId);

        using var reader = command.ExecuteReader();
        var orders = new List<Order>();
        while (reader.Read())
        {
            orders.Add(Map(reader));
        }
        return orders;
    }

    public Order? GetOrder(long customerId, long orderId)
    {
        using var connection = _db.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT id, customer_id, description, total, status, placed_on " +
            "FROM orders WHERE id = @orderId AND customer_id = @customerId";
        command.Parameters.AddWithValue("@orderId", orderId);
        command.Parameters.AddWithValue("@customerId", customerId);

        using var reader = command.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    private static Order Map(SqliteDataReader reader)
    {
        return new Order(
            reader.GetInt64(0),
            reader.GetInt64(1),
            reader.GetString(2),
            Convert.ToDecimal(reader.GetDouble(3)),
            reader.GetString(4),
            DateTime.Parse(reader.GetString(5), CultureInfo.InvariantCulture));
    }
}
