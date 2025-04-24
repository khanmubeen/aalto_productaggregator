using Aalto.ProductAggregator.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using System.Net.Http;

//skeleton code to get startin with API call 
////invoke product service
//var productService = new ProductService(new HttpClient());

////get list from api product service
//var products = await productService.GetProductsAsync();

////debug
//foreach (var product in products)
//{
//    Console.WriteLine($"ID: {product.Id}, Title: {product.Title}, Price: {product.Price} , Rate: {product.Rating.Rate}");
//}

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddHttpClient("FakeStoreClient", client =>
        {
            client.BaseAddress = new Uri("https://fakestoreapi.com/");
        })
        .AddPolicyHandler(Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)))
        );

        services.AddTransient<ProductService>();
        services.AddTransient<FileService>();
    })
    .Build();

var scope = host.Services.CreateScope();
var productService = scope.ServiceProvider.GetService<ProductService>();
var fileService = scope.ServiceProvider.GetService<FileService>();

// Parse command-line arguments
bool saveAsJson = args.Contains("--json");
bool saveAsCsv = args.Contains("--csv");
double? minPrice = null;
double? maxPrice = null;

foreach (var arg in args)
{
    if (arg.StartsWith("--minPrice="))
        minPrice = double.TryParse(arg.Split("=")[1], out var val) ? val : null;
    if (arg.StartsWith("--maxPrice="))
        maxPrice = double.TryParse(arg.Split("=")[1], out var val) ? val : null;
}


try
{
    var products = await productService.GetProductsAsync();

    // Filter products by minPrice if specified
    if (minPrice > 0)
    {
        products = products.Where(p => p.Price >= minPrice).ToList();
    }
    if (maxPrice > 0)
    { 
        products = products.Where(p => p.Price <= maxPrice).ToList();    
    }

    var enriched = productService.EnrichData(products);

    var grouped = productService.GroupAndSort(enriched);
    //Debug
    //foreach (var product in products)
    //{
    //    Console.WriteLine($"ID: {product.Id}, Title: {product.Title}, Price: {product.Price} , Rate: {product.Rating.Rate}");
    //}

    //File Service save as json and csv

    if (saveAsJson && !saveAsCsv)
    {
        fileService.SaveAsJson(grouped, "grouped_products.json");
        return;
    }
    if (saveAsCsv && !saveAsJson)
    {
        fileService.SaveAsCsv(grouped, "grouped_products.csv");
        return;
    }
    else 
    {
        fileService.SaveAsJson(grouped, "grouped_products.json");
        fileService.SaveAsCsv(grouped, "grouped_products.csv");
    }
    
}
catch (Exception ex) 
{ 
    Console.WriteLine(ex);
}