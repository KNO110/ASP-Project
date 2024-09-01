using ASP_P15.Services;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text;

namespace ASP_P15.Controllers
{
    public class FileDownloadController : Controller
    {
        private readonly IFileNameGeneratorService _fileNameGeneratorService;

        public FileDownloadController(IFileNameGeneratorService fileNameGeneratorService)
        {
            _fileNameGeneratorService = fileNameGeneratorService;
        }

        [HttpPost]
        public IActionResult GenerateAndDownloadFile(int length)
        {
            //// генерим имя файла
            var fileName = _fileNameGeneratorService.GenerateFileName(length) + ".txt";
            var fileContent = "Сюда можно типо пароль с даными акка добавить для авторизированного челика. Или по типу как в дискорде, ключи доступа 20 штук для восстановления акка и просто для защиты. Но мне лень, тем более задача не требует:)";

            ////// Создаём поток с содержимым файла
            var fileBytes = Encoding.UTF8.GetBytes(fileContent);
            var fileStream = new MemoryStream(fileBytes);

            ////// return файла для скачивания
            return File(fileStream, "text/plain", fileName);
        }
    }
}
