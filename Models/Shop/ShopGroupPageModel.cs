using ASP_P15.Data.Entities;

namespace ASP_P15.Models.Shop
{
    public class ShopGroupPageModel
    {
        public ProductGroup ProductGroup { get; set; } = null!;
        public IEnumerable<ProductGroup> Groups { get; set; } = null!;
        public List<double> PriceLimits { get; set; } = new List<double>();
    }

}
