using Microsoft.AspNetCore.Mvc;

namespace AcmePortal.Web.Controllers;

public abstract class PortalController : Controller
{
    public const string CustomerSessionKey = "CustomerId";

    protected long? CurrentCustomerId => HttpContext.Session.GetInt32(CustomerSessionKey);
}
