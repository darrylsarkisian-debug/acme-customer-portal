using AcmePortal.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace AcmePortal.Web.Controllers;

public class AccountController : PortalController
{
    private readonly OrderService _orders;

    public AccountController(OrderService orders)
    {
        _orders = orders;
    }

    [HttpGet]
    public IActionResult Login() => View();

    // Legacy: this POST does not validate an antiforgery token yet.
    [HttpPost]
    public IActionResult Login(string email)
    {
        var customer = string.IsNullOrWhiteSpace(email)
            ? null
            : _orders.FindCustomerByEmail(email.Trim());

        if (customer is null)
        {
            ModelState.AddModelError(string.Empty, "We could not find an account for that email.");
            return View();
        }

        HttpContext.Session.SetInt32(CustomerSessionKey, (int)customer.Id);
        return RedirectToAction("Index", "Orders");
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}
