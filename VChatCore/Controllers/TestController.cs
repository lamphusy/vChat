using Microsoft.AspNetCore.Mvc;

namespace VChatCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "API already";
        }
    }
}
