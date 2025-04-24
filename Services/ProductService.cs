using Aalto.ProductAggregator.Models;
using Newtonsoft.Json;

namespace Aalto.ProductAggregator.Services
{
    public class ProductService
    {
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "https://fakestoreapi.com/products";
        private const string CacheFile = "cache.json";

        public ProductService(IHttpClientFactory httpClientFactory)
        {
            //_httpClient = httpClient;
            _httpClient = httpClientFactory.CreateClient("FakeStoreClient");
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            //var response = await _httpClient.GetStringAsync(ApiUrl);
            //return JsonConvert.DeserializeObject<List<Product>>(response);

            var response = await _httpClient.GetAsync("products");

            // Ensure the request was successful (status code 2xx), otherwise throw an exception
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var products = JsonConvert.DeserializeObject<List<Product>>(json) ?? new List<Product>();

            //Save to cache

            return products;
        }

        public async Task<Product> GetProductByIdAsync(string id)
        {
            var response = await _httpClient.GetStringAsync($"{ApiUrl}/{id}");
            return JsonConvert.DeserializeObject<Product>(response);
        }

        public List<Product> EnrichData(List<Product> products)
        {
            Random rnd = new();

            foreach (var product in products)
            {
                var discountPercent = rnd.Next(5, 21);
                product.DiscountedPrice = Math.Round(product.Price * (1 - discountPercent / 100.0), 2);
                product.Stock = rnd.Next(0, 101);
                product.PopularityScore = CalculatePopularityScore(product);
            }

            return products;

        }

        private int CalculatePopularityScore(Product product)
        {
            // Simple popularity logic: more stock & higher price = more popular
            return (int)(product.Stock * (product.Price / 100.0));
        }

        public Dictionary<string, List<Product>> GroupAndSort(List<Product> products)
        {
            return products
                .GroupBy(p => p.Category)
                .ToDictionary(
                    group => group.Key,
                    group => group.OrderByDescending(p => p.Price).ToList()
                );
        }
    }
}
