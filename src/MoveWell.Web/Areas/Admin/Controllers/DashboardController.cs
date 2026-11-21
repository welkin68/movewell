using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoveWell.Web.DependencyInjection;

namespace MoveWell.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = WebServiceCollectionExtensions.StaffPolicy)]
public sealed class DashboardController : Controller
{
    public IActionResult Index() => View();
}
