namespace Aalto.ProductAggregator.Models
{
    public class Rating
    {
        public double Rate { get; set; }
        public int Count { get; set; }
    }
    public class Product
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public double Price { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Image { get; set; }
        public Rating Rating { get; set; }

        // Enriched Fields
        public double DiscountedPrice { get; set; }
        public int Stock { get; set; }
        public int PopularityScore { get; set; }
    }
}