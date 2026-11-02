using Microsoft.AspNetCore.Mvc;

namespace PaquitoBot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok();
    }

    [HttpPost]
    public IActionResult Post()
    {
        return Ok();
    }
}