using ASP_P15.Data;
using ASP_P15.Data.Entities;
using ASP_P15.Models.Api;
using ASP_P15.Models.Cart;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASP_P15.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartController(DataContext dataContext) : ControllerBase
    {
        private readonly DataContext _dataContext = dataContext;

        [HttpGet]
        public RestResponse<Cart?> DoGet([FromQuery] String id)
        {
            RestResponse<Cart?> response = new()
            {
                Meta = new()
                {
                    Service = "Cart",
                },
                Data = _dataContext
                        .Carts
                        .Include(c => c.CartProducts)
                            .ThenInclude(cp => cp.Product)
                        .FirstOrDefault(c =>
                            c.UserId.ToString() == id &&
                            c.CloseDt == null &&
                            c.DeleteDt == null)
            };

            return response;
        }

        [HttpPost]
        public async Task<IActionResult> DoPost([FromBody] CartFormModel formModel)
        {
            if (formModel.UserId == default)
            {
                return Unauthorized(new { status = "Error", message = "Ви не авторизовані." });
            }
            if (formModel.ProductId == default)
            {
                return BadRequest(new { status = "Error", message = "Відсутній ідентифікатор товару." });
            }
            if (formModel.Cnt <= 0)
            {
                return BadRequest(new { status = "Error", message = "Кількість повинна бути більше нуля." });
            }

            var cart = _dataContext
                .Carts
                .FirstOrDefault(c =>
                    c.UserId == formModel.UserId &&
                    c.CloseDt == null &&
                    c.DeleteDt == null);

            if (cart == null)
            {
                Guid cartId = Guid.NewGuid();
                cart = new Cart
                {
                    Id = cartId,
                    UserId = formModel.UserId,
                    CreateDt = DateTime.Now,
                };
                _dataContext.Carts.Add(cart);
            }

            var cartProduct = _dataContext
                .CartProducts
                .FirstOrDefault(cp =>
                    cp.CartId == cart.Id &&
                    cp.ProductId == formModel.ProductId);

            if (cartProduct == null)
            {
                cartProduct = new CartProduct
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    ProductId = formModel.ProductId,
                    Cnt = formModel.Cnt,
                };
                _dataContext.CartProducts.Add(cartProduct);
            }
            else
            {
                cartProduct.Cnt += formModel.Cnt;
            }

            await _dataContext.SaveChangesAsync();

            int totalItems = _dataContext.CartProducts
                .Include(cp => cp.Cart)
                .Where(cp => cp.Cart.UserId == formModel.UserId && cp.Cart.CloseDt == null && cp.Cart.DeleteDt == null)
                .Sum(cp => cp.Cnt);

            return Ok(new { status = "OK", totalItems = totalItems });
        }



        [HttpPut]
        public async Task<RestResponse<String>> DoPut(
            [FromQuery] Guid cpId, [FromQuery] int increment)
        {
            RestResponse<String> response = new()
            {
                Meta = new()
                {
                    Service = "Cart",
                },
            };
            if (cpId == default)
            {
                response.Data = "Error 400: cpId is not valid";
                return response;
            }
            if (increment == 0)
            {
                response.Data = "Error 400: increment is not valid";
                return response;
            }
            var cp = _dataContext
                .CartProducts
                .Include(cp => cp.Cart)
                .FirstOrDefault(cp => cp.Id == cpId);
            if (cp == null)
            {
                response.Data = "Error 404: cpId does not identify entity";
                return response;
            }
            if (cp.Cart.CloseDt is not null || cp.Cart.DeleteDt is not null)
            {
                response.Data = "Error 409: cpId identifies not active entity";
                return response;
            }
            if (cp.Cnt + increment < 0)
            {
                response.Data = "Error 422: increment could not be applied";
                return response;
            }

            if (cp.Cnt + increment == 0)
            {
                // віднімання усього -- видалення 
                _dataContext.CartProducts.Remove(cp);
                response.Meta.Count = 0;
            }
            else
            {
                // оновлення кількості
                cp.Cnt += increment;
                response.Meta.Count = cp.Cnt;
            }
            await _dataContext.SaveChangesAsync();
            response.Data = "Updated";
            return response;
        }


    }
}
/* Реалізувати виведення повідомлень щодо успішності додавання
 * товару до кошику (додано успішно / помилка додавання).
 * ** Також виводити кількість товару у кошику:
 *     Додано успішно, у кошику 3 шт обраних вами товарів (всього - 10)
 */
