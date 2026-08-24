using Microsoft.AspNetCore.Mvc;

namespace JobTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            status = "Healthy",
            application = "JobTrack API",
            timestamp = DateTime.UtcNow
        });
    }
}
