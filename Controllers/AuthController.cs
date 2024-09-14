using ASP_P15.Data;
using ASP_P15.Data.Entities;
using ASP_P15.Services.Kdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace ASP_P15.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IKdfService _kdfService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(DataContext dataContext, IKdfService kdfService, ILogger<AuthController> logger)
        {
            _dataContext = dataContext;
            _kdfService = kdfService;
            _logger = logger;
        }

        [HttpGet]
        public object DoGet(String email, String password)
        {
            if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(password))
            {
                return new
                {
                    status = "Error",
                    code = 400,
                    message = "Email and password must not be empty"
                };
            }

            // Находим пользователя по email
            var user = _dataContext
                .Users
                .FirstOrDefault(u =>
                    u.Email == email &&
                    u.DeleteDt == null);  // проверка, что пользователь не удален

            if (user != null && _kdfService.DerivedKey(password, user.Salt) == user.Dk)
            {
                // Проверяем, есть ли активный токен для пользователя
                var activeToken = _dataContext.Tokens
                    .FirstOrDefault(t =>
                        t.UserId == user.Id &&
                        t.ExpiresAt > DateTime.Now);  // проверяем, что токен не истек

                if (activeToken != null)
                {
                    // Если есть активный токен, возвращаем его
                    HttpContext.Session.SetString("token", activeToken.Id.ToString());
                    return new
                    {
                        status = "Ok",
                        code = 200,
                        message = activeToken.Id  // возвращаем активный токен клиенту
                    };
                }
                else
                {
                    // Если активного токена нет, генерируем новый
                    Token newToken = new()
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        ExpiresAt = DateTime.Now.AddHours(3),
                    };
                    _dataContext.Tokens.Add(newToken);
                    _dataContext.SaveChanges();
                    HttpContext.Session.SetString("token", newToken.Id.ToString());
                    return new
                    {
                        status = "Ok",
                        code = 200,
                        message = newToken.Id  //// возвращаем новый токен клиенту
                    };
                }
            }
            else
            {
                return new
                {
                    status = "Reject",
                    code = 401,
                    message = "Credentials rejected"
                };
            }
        }

        [HttpDelete]
        public object DoDelete()
        {
            HttpContext.Session.Remove("token");
            return "Ok";
        }

        [HttpPut]
        public async Task<object> DoPutAsync()
        {
            ///// Дані, що передаються в тілі запиту доступні через Request.Body
            String body = await new StreamReader(Request.Body).ReadToEndAsync();

            _logger.LogWarning(body);

            JsonNode json = JsonSerializer.Deserialize<JsonNode>(body)
                ?? throw new Exception("JSON in body is invalid");

            String? email = json["email"]?.GetValue<String>();
            String? name = json["name"]?.GetValue<String>();
            String? birthdate = json["birthdate"]?.GetValue<String>();

            if (email == null && name == null && birthdate == null)
            {
                return new { code = 400, status = "Error", message = "No data" };
            }
            if (email != null)
            {
                var emailRegex = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
                if (!emailRegex.IsMatch(email))
                {
                    return new { code = 422, status = "Error", message = "Email match no pattern" };
                }
            }
            DateTime? birthDateTime = null;
            if (birthdate != null)
            {
                try
                {
                    birthDateTime = DateTime.Parse(birthdate);
                }
                catch
                {
                    return new { code = 422, status = "Error", message = "Birthdate unparseable" };
                }
            }

            Guid userId = Guid.Parse(
                HttpContext
                .User
                .Claims
                .First(c => c.Type == ClaimTypes.Sid)
                .Value);

            var user = _dataContext.Users.Find(userId);
            if (user == null)
            {
                return new { code = 403, status = "Error", message = "Forbidden" };
            }

            if (email != null)
            {
                user.Email = email;
            }
            if (name != null)
            {
                user.Name = name;
            }
            if (birthDateTime != null)
            {
                user.Birthdate = birthDateTime;
            }

            await _dataContext.SaveChangesAsync();

            return new { code = 200, status = "OK", message = "Updated" };
        }

        private async Task<object> DoLink()
        {
            String body = await new StreamReader(Request.Body).ReadToEndAsync();

            _logger.LogWarning(body);

            JsonNode json = JsonSerializer.Deserialize<JsonNode>(body)
                ?? throw new Exception("JSON in body is invalid");

            String? email = json["email"]?.GetValue<String>();
            String? password = json["password"]?.GetValue<String>();
            String? dateStr = json["date"]?.GetValue<String>();

            if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(password) || String.IsNullOrEmpty(dateStr))
            {
                return new
                {
                    status = "Error",
                    code = 400,
                    message = "Email, password and date must not be empty"
                };
            }

            DateTime date;
            try
            {
                date = DateTime.Parse(dateStr);
            }
            catch
            {
                return new { code = 422, status = "Error", message = "Date unparseable" };
            }

            ///// ищем юзера по емейлу даже если он был "удалён"
            var user = _dataContext
                .Users
                .FirstOrDefault(u =>
                    u.Email == email
                ); 

            if (user == null)
            {
                return new { code = 404, status = "Error", message = "User not found" };
            }

            //// проверяем соответсвие введённой даты с настоящей
            if (user.Registered.Date != date.Date)
            {
                return new { code = 401, status = "Error", message = "Credentials rejected" };
            }

            //// проверяем пароль
            if (_kdfService.DerivedKey(password, user.Salt) != user.Dk)
            {
                return new { code = 401, status = "Error", message = "Credentials rejected" };
            }

            ////воскрешаем юзера
            user.DeleteDt = null;

            await _dataContext.SaveChangesAsync();

            ///генерим новый токен
            Token newToken = new()
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                ExpiresAt = DateTime.Now.AddHours(3),
            };
            _dataContext.Tokens.Add(newToken);
            await _dataContext.SaveChangesAsync();
            HttpContext.Session.SetString("token", newToken.Id.ToString());

            return new
            {
                status = "Ok",
                code = 200,
                message = "Account restored",
                token = newToken.Id.ToString()
            };
        }


        public async Task<object> DoOther()
        {
            switch (Request.Method)
            {
                case "UNLINK": return await DoUnlink();
                case "LINK": return await DoLink();
                default:
                    return new
                    {
                        status = "error",
                        code = 405,
                        message = "Method Not Allowed"
                    };
            }
        }

        private async Task<object> DoUnlink()
        {
            Guid userId;
            try
            {
                userId = Guid.Parse(
                    HttpContext
                    .User
                    .Claims
                    .First(c => c.Type == ClaimTypes.Sid)
                    .Value
                );
            }
            catch (Exception ex)
            {
                _logger.LogError("DoUnlink Exception: {ex}", ex.Message);
                return new
                {
                    code = 401,
                    status = "Error",
                    message = "UnAuthorized"
                };
            }

            var user = await _dataContext.Users.FindAsync(userId);
            if (user == null)
            {
                return new { code = 403, status = "Error", message = "Forbidden" };
            }

            user.DeleteDt = DateTime.Now;
            // Do not delete personal data to allow recovery
            // Do not delete the avatar
            await _dataContext.SaveChangesAsync();   // Save changes to the database
            this.DoDelete();   // Remove the token
            return new
            {
                status = "OK",
                code = 200,
                message = "Deleted",
                registeredDate = user.Registered.ToString("yyyy-MM-dd")
            };
        }
    }
}
/*
 * Контролери розрізняють: MVC та API
 * MVC - різні адреси ведуть на різні дії (actions)
 *    /Home/Index -> Index()
 *    /Home/Db    -> Db()
 *    
 * API - різні методи запиту ведуть на різні дії
 *   GET  /api/auth  -> DoGet()
 *   POST /api/auth  -> DoPost()
 *   PUT  /api/auth  -> DoPut()
 *   
 *   
 * Токени авторизації  
 * Токен - "жетон", "перепустка" - дані, що видаються як результат
 * автентифікації і далі використовуються для "підтвердження особи" -
 * авторизації.
 *   
 *   
 */
/* CRUD: Delete
 * Особливості видалення даних
 * ! видалення створює проблеми за наявності зв'язків між даними
 * - замість видалення вводиться мітка "видалено" (у вигляді дати-часу видалення)
 * ! Art. 17 GDPR "Право бути забутим" - необхідність видалення персональних
 *   даних на вимогу користувача
 * - Класифікувати дані на персональні / не персональні, одні - видаляти, інші
 *   залишати.
 *   
 * = розглядається два варіанти видалень
 *  soft-delete - помітка видалення і у випадку людини стирання персональних даних
 *  hard-delete - повне видалення - допускається лише за відсутності зв'язків
 */
/* Д.З. Реалізувати відновлення користувача:
 * - при видаленні видавати повідомлення "Для відновлення введіть дату
 *    реєстрації ([вивести дату]) та свій пароль"
 * - до вікна авторизації додати кнопку "відновити" яка додає поле з 
 *    введенням дати реєстрації (всього три поля - e-mail, пароль, дата)
 * - при натисненні кнопки передається запит методом LINK, у якому
 *    перевіряється дата та пароль, якщо ОК - зберігається e-mail та
 *    скидається дата видалення.
 */
