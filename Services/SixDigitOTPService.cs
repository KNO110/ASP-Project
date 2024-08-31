using System;
using System.Linq;

namespace ASP_P15.Services
{
    public class SixDigitOTPService : IOTPService
    {

        public string GenerateOTP()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
