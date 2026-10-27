using AcmePortal.Web.Data;
using AcmePortal.Web.Services;
using Xunit;

namespace AcmePortal.Tests;

public class OrderServiceTests : IDisposable
{
    private readonly string _dbPath =
        Path.Combine(Path.GetTempPath(), $"acme-portal-test-{Guid.NewGuid():N}.db");

    private readonly OrderService _service;

    public OrderServiceTests()
    {
        var db = new Database($"Data Source={_dbPath};Pooling=False");
        db.Initialize();
        _service = new OrderService(db);
    }

    [Fact]
    public void FindCustomerByEmail_ReturnsSeededCustomer()
    {
        var customer = _service.FindCustomerByEmail("dana@acme.example");

        Assert.NotNull(customer);
        Assert.Equal("Dana Whitfield", customer!.Name);
    }

    [Fact]
    public void FindCustomerByEmail_ReturnsNullForUnknownEmail()
    {
        Assert.Null(_service.FindCustomerByEmail("nobody@acme.example"));
    }

    [Fact]
    public void GetOrdersForCustomer_ReturnsOnlyThatCustomersOrders()
    {
        var orders = _service.GetOrdersForCustomer(1);

        Assert.Equal(3, orders.Count);
        Assert.All(orders, order => Assert.Equal(1, order.CustomerId));
    }

    [Fact]
    public void GetOrder_ForAnotherCustomer_ReturnsNull()
    {
        // Order 1004 belongs to customer 2.
        Assert.Null(_service.GetOrder(customerId: 1, orderId: 1004));
    }

    public void Dispose()
    {
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }
}
