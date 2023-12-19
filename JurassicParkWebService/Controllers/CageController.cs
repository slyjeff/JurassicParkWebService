using Microsoft.AspNetCore.Mvc;

namespace JurassicParkWebService.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class CageController : ControllerBase {
        [HttpGet]
        public IActionResult Get() {
            return StatusCode(200);
        }
    }
}