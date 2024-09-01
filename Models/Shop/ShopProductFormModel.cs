namespace ASP_P15.Models.Shop
{
    public class ShopProductFormModel
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public IFormFile ImageFile { get; set; }
        public double Price { get; set; }
        public long Amount { get; set; }
        public Guid GroupId { get; set; }
        public string? Slug { get; set; }
    }
}
