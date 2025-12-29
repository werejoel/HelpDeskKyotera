using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskKyotera.Controllers;

[Authorize(Roles = "Admin,Staff,CEO")]
public class ChartsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
