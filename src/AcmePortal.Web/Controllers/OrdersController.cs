using AcmePortal.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace AcmePortal.Web.Controllers;

public class OrdersController : PortalController
{
    private readonly OrderService _orders;

    public OrdersController(OrderService orders)
    {
        _orders = orders;
    }

    public IActionResult Index()
    {
        if (CurrentCustomerId is not long customerId)
        {
            return RedirectToAction("Login", "Account");
        }

        return View(_orders.GetOrdersForCustomer(customerId));
    }

    public IActionResult Details(long id)
    {
        if (CurrentCustomerId is not long customerId)
        {
            return RedirectToAction("Login", "Account");
        }

        var order = _orders.GetOrder(customerId, id);
        return order is null ? NotFound() : View(order);
    }
}
