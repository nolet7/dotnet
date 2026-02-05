using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers;

[ApiController]
[Route("")]
public class InventoryController : ControllerBase
{
    [HttpPost("check")]
    public IActionResult Check()
    {
        return Ok("Stock available");
    }
}

