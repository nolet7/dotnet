using Microsoft.AspNetCore.Mvc;

namespace PaymentService.Controllers;

[ApiController]
[Route("")]
public class PaymentController : ControllerBase
{
    [HttpPost("pay")]
    public IActionResult Pay()
    {
        return Ok("Payment successful");
    }
}

