using ASP_P15.Data;
using ASP_P15.Services.Kdf;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class ProfileController : Controller
{
    private readonly DataContext _dataContext;
    private readonly IKdfService _kdfService;

    public ProfileController(DataContext dataContext, IKdfService kdfService)
    {
        _dataContext = dataContext;
        _kdfService = kdfService;
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword)
    {
        var userId = User.FindFirst(ClaimTypes.Sid)?.Value;
        if (userId == null)
        {
            return Json(new { success = false, error = "Пользователь не авторизован." });
        }

        var user = _dataContext.Users.FirstOrDefault(u => u.Id == Guid.Parse(userId));
        if (user == null)
        {
            return Json(new { success = false, error = "Пользователь не найден." });
        }

        ///// Проверка старого пароля
        if (_kdfService.DerivedKey(oldPassword, user.Salt) != user.Dk)
        {
            return Json(new { success = false, error = "Старый пароль введен неверно." });
        }

        /// Обнова пароля
        user.Dk = _kdfService.DerivedKey(newPassword, user.Salt);
        await _dataContext.SaveChangesAsync();

        return Json(new { success = true });
    }

}
