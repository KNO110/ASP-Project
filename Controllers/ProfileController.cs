using ASP_P15.Data;
using ASP_P15.Services.Kdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

public class ProfileController : Controller
{
    private readonly DataContext _dataContext;
    private readonly IKdfService _kdfService;
    private readonly ILogger<ProfileController> _logger; // Добавляем логгер

    public ProfileController(DataContext dataContext, IKdfService kdfService, ILogger<ProfileController> logger)
    {
        _dataContext = dataContext;
        _kdfService = kdfService;
        _logger = logger; //// Инициализируем логггер
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword)
    {
        //// Логирование полученных данных
        _logger.LogInformation("Получен запрос на смену пароля.");
        _logger.LogInformation("Старый пароль: {OldPassword}", oldPassword);
        _logger.LogInformation("Новый пароль: {NewPassword}", newPassword);

        var userId = User.FindFirst(ClaimTypes.Sid)?.Value;
        if (userId == null)
        {
            _logger.LogWarning("Пользователь не авторизован.");
            return Json(new { success = false, error = "Пользователь не авторизован." });
        }

        var user = _dataContext.Users.FirstOrDefault(u => u.Id == Guid.Parse(userId));
        if (user == null)
        {
            _logger.LogWarning("Пользователь не найден: {UserId}", userId);
            return Json(new { success = false, error = "Пользователь не найден." });
        }

        //// Проверка старого пароля
        if (_kdfService.DerivedKey(oldPassword, user.Salt) != user.Dk)
        {
            _logger.LogWarning("Введен неверный старый пароль для пользователя: {UserId}", userId);
            return Json(new { success = false, error = "Старый пароль введен неверно." });
        }

        //// Обновление пароля
        user.Dk = _kdfService.DerivedKey(newPassword, user.Salt);
        await _dataContext.SaveChangesAsync();

        _logger.LogInformation("Пароль успешно изменен для пользователя: {UserId}", userId);
        return Json(new { success = true });
    }
}
