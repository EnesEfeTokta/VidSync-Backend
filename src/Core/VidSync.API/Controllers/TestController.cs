using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VidSync.Domain.Interfaces;

namespace VidSync.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public TestController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("API is working!");
        }

        [HttpGet("send-test-email")]
        public async Task<IActionResult> SendTestEmail()
        {
            await _emailService.SendEmailAsync("enesefetkta009@gmail.com", "Test Email", "<h1>This is a test email</h1>");
            return Ok("Test email sent.");
        }
    }
}
