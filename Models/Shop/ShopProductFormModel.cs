using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ASP_P15.Models.Shop
{
    public class ShopProductFormModel
    {
        [FromForm(Name = "product-name")]
        [Required(ErrorMessage = "Назва товару обов'язкова.")]
        [StringLength(100, ErrorMessage = "Назва товару повинна бути не довше 100 символів.")]
        public string Name { get; set; } = null!;

        [FromForm(Name = "product-description")]
        [Required(ErrorMessage = "Опис товару обов'язковий.")]
        [StringLength(500, ErrorMessage = "Опис товару повинен бути не довше 500 символів.")]
        public string Description { get; set; } = null!;

        [FromForm(Name = "product-slug")]
        [StringLength(50, ErrorMessage = "Slug повинен бути не довше 50 символів.")]
        public string? Slug { get; set; }

        [FromForm(Name = "product-price")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Ціна повинна бути більше нуля.")]
        public double Price { get; set; }

        [FromForm(Name = "product-amount")]
        [Range(0, int.MaxValue, ErrorMessage = "Кількість не може бути від'ємною.")]
        public int Amount { get; set; }

        [FromForm(Name = "product-picture")]
        [JsonIgnore]
        public IFormFile ImageFile { get; set; } = null!;

        [FromForm(Name = "group-id")]
        public Guid GroupId { get; set; }
    }
}
