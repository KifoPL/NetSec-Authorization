using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NetSec.MVC.Controllers;

[Controller]
[Route("[controller]")]
[Authorize]
public class SecretController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var howToGetGoodAtCyberSec = new[]
        {
            "Read a lot of books",
            "Learn from other people",
            "Learn from your own mistakes",
            "Attend at NetSec meetings :)"
        };

        return View(howToGetGoodAtCyberSec);
    }
}