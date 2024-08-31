using ASP_P15.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASP_P15.Controllers
{
    public class OTPController : Controller
    {
        private readonly IOTPService _otpService;

        public OTPController(IOTPService otpService)
        {
            _otpService = otpService;
        }

        [HttpGet]
        public IActionResult Generate()
        {
            var otp = _otpService.GenerateOTP();
            return Content(otp);
        }
    }
}
