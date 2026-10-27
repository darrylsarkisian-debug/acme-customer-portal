using Microsoft.AspNetCore.Mvc;

namespace AcmePortal.Web.Controllers;

public class HomeController : PortalController
{
    public IActionResult Index() => View();
}
