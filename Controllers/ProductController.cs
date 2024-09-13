using ASP_P15.Data;
using ASP_P15.Models.Group;
using ASP_P15.Models.Shop;
using ASP_P15.Services.Upload;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASP_P15.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class ProductController(IFileUploader fileUploader, DataContext dataContext) : ControllerBase
    {
        private readonly IFileUploader _fileUploader = fileUploader;
        private readonly DataContext _dataContext = dataContext;

        [HttpPost]
        public async Task<object> DoPost(ShopProductFormModel formModel)
        {
                        ////////// Валидация данных
            if (string.IsNullOrWhiteSpace(formModel.Name))
            {
                return new { code = 400, status = "error", message = "Назва товару обов'язкова." };
            }

            if (formModel.Price <= 0)
            {
                return new { code = 400, status = "error", message = "Ціна повинна бути більше нуля." };
            }

            if (formModel.Amount < 0)
            {
                return new { code = 400, status = "error", message = "Кількість не може бути від'ємною." };
            }

            String uploadedName;
            try
            {
                        // грузим файлы
                uploadedName = _fileUploader.UploadFile(
                    formModel.ImageFile,
                    "./Uploads/Shop"
                );
    
                ///добавление в бд новые данные
                _dataContext.Products.Add(new()
                {
                    Id = Guid.NewGuid(),
                    Name = formModel.Name,
                    Description = formModel.Description,
                    Image = uploadedName,
                    DeleteDt = null,
                    Slug = formModel.Slug,
                    Price = formModel.Price,
                    Amount = formModel.Amount,
                    GroupId = formModel.GroupId,
                });

                await _dataContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //// ловим ашибку
                return new { code = 500, status = "error", message = ex.Message };
            }

            //// всё топчик цины горобчик
            return new { code = 200, status = "OK", message = "Товар успішно створено." };
        }
    }
}
