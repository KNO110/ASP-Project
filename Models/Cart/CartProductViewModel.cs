using Microsoft.AspNetCore.Mvc;

namespace ASP_P15.Models.Cart
{
    public class CartProductViewModel : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
