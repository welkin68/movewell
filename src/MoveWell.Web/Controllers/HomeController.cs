using Microsoft.AspNetCore.Mvc;

namespace MoveWell.Web.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index() => View();

    /// <summary>Exception handler (500) and status-code pages (404 …) re-execute here.</summary>
    [Route("error")]
    [Route("error/{code:int}")]
    [IgnoreAntiforgeryToken]
    public IActionResult Error(int? code)
    {
        Response.StatusCode = code ?? StatusCodes.Status500InternalServerError;
        return View("Error", code);
    }
}
