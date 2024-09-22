using ASP_P15.Data;
using ASP_P15.Services.Kdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.IO;
using Microsoft.AspNetCore.Http;

public class ProfileController : Controller
{
    private readonly DataContext _dataContext;
    private readonly IKdfService _kdfService;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(DataContext dataContext, IKdfService kdfService, ILogger<ProfileController> logger)
    {
        _dataContext = dataContext;
        _kdfService = kdfService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult CreditCardForm()
    {
        var userId = User.FindFirst(ClaimTypes.Sid)?.Value;
        var user = _dataContext.Users.FirstOrDefault(u => u.Id == Guid.Parse(userId));

        if (user == null)
        {
            return NotFound();
        }

        // Если карта уже добавлена, устанавливаем сообщение в TempData
        if (!string.IsNullOrEmpty(user.CardNumber))
        {
            TempData["WarningMessage"] = "You already have a credit card added. You can update it if necessary.";
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SubmitCreditCard(string cardNumber, string cardOwner, string expiryDate)
    {
        cardNumber = cardNumber.Replace(" ", "");


        if (cardNumber.Length != 16)
        {
            TempData["ErrorMessage"] = "Invalid card number. Must be 16 digits.";
            return RedirectToAction("CreditCardForm");
        }


        var parts = expiryDate.Split('/');
        if (parts.Length != 2 || parts[0].Length != 2 || parts[1].Length != 2)
        {
            TempData["ErrorMessage"] = "Invalid expiry date format. Use MM/YY.";
            return RedirectToAction("CreditCardForm");
        }

        int month = int.Parse(parts[0]);
        int year = int.Parse(parts[1]) + 2000; 

        if (month < 1 || month > 12)
        {
            TempData["ErrorMessage"] = "Invalid month. Please enter a valid month (01-12).";
            return RedirectToAction("CreditCardForm");
        }

        int currentYear = DateTime.Now.Year;
        int currentMonth = DateTime.Now.Month;

        if (year < currentYear || (year == currentYear && month < currentMonth))
        {
            TempData["ErrorMessage"] = "Invalid year. The expiration date cannot be in the past.";
            return RedirectToAction("CreditCardForm");
        }

        var userId = User.FindFirst(ClaimTypes.Sid)?.Value;
        var user = _dataContext.Users.FirstOrDefault(u => u.Id == Guid.Parse(userId));

        if (user == null)
        {
            return NotFound();
        }

        user.CardNumber = cardNumber;
        user.CardOwner = cardOwner;
        user.ExpiryDate = expiryDate;

        _dataContext.Users.Update(user);
        await _dataContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Credit card information saved successfully!";
        return RedirectToAction("CreditCardForm");
    }




    [HttpPost]
    public async Task<IActionResult> ChangeAvatar(IFormFile avatar)
    {
        var userId = User.FindFirst(ClaimTypes.Sid)?.Value;
        if (userId == null)
        {
            _logger.LogWarning("Користувач не автентифікований.");
            return Json(new { success = false, error = "Користувач не у системі." });
        }

        var user = _dataContext.Users.FirstOrDefault(u => u.Id == Guid.Parse(userId));
        if (user == null)
        {
            _logger.LogWarning("Користувач не знайден: {UserId}", userId);
            return Json(new { success = false, error = "користувач не був знайден." });
        }

        if (avatar == null || avatar.Length == 0)
        {
            _logger.LogWarning("Файл аватар не доданий.");
            return Json(new { success = false, error = "Файл аватара не доданий." });
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(avatar.FileName).ToLower();
        if (!allowedExtensions.Contains(extension))
        {
            _logger.LogWarning("Цей формат файлу не підтримуєтся.");
            return Json(new { success = false, error = "Цей формат файлу не підтримуєтся." });
        }

        ////Сохраняем аватар
        string fileName = Guid.NewGuid().ToString() + extension;
        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }
        string filePath = Path.Combine(uploadsFolder, fileName);
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await avatar.CopyToAsync(fileStream);
        }

        /////Удаляем прошлую личность, ой то есть аватар если такой имеется
        if (!string.IsNullOrEmpty(user.Avatar))
        {
            string oldFilePath = Path.Combine(uploadsFolder, user.Avatar);
            if (System.IO.File.Exists(oldFilePath))
            {
                try
                {
                    System.IO.File.Delete(oldFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Помилка при видалені старого аватару: {Message}", ex.Message);
                }
            }
        }

        /// Обновляем на текущий аватар для нашего пользователя
        user.Avatar = fileName;
        await _dataContext.SaveChangesAsync();

        _logger.LogInformation("Аватар успішно змніено для користувача: {UserId}", userId);
        return Json(new { success = true });
    }
}
