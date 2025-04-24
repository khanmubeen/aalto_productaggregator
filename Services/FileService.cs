using Aalto.ProductAggregator.Models;
using Newtonsoft.Json;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace Aalto.ProductAggregator.Services;

public class FileService
{
    private const string OutputFolderName = "Files_Reports";

    private string GetOutputFolderPath()
    {
        var projectRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\.."));
        var outputFolder = Path.Combine(projectRoot, OutputFolderName);
        Directory.CreateDirectory(outputFolder); // Ensure it exists
        return outputFolder;
    }

    public void SaveAsJson(Dictionary<string, List<Product>> groupedProducts, string fileName)
    {
        if (groupedProducts == null || !groupedProducts.Any())
        {
            Console.WriteLine("No data to save as JSON.");
            return;
        }
        var fullPath = Path.Combine(GetOutputFolderPath(), fileName);

        var json = JsonConvert.SerializeObject(groupedProducts, Formatting.Indented);
        File.WriteAllText(fullPath, json);

        Console.WriteLine($"JSON saved to {fileName}");
    }

    public void SaveAsCsv(Dictionary<string, List<Product>> groupedProducts, string fileName)
    {
        if (groupedProducts == null || !groupedProducts.Any())
        {
            Console.WriteLine("No data to save as CSV.");
            return;
        }

        var fullPath = Path.Combine(GetOutputFolderPath(), fileName);

        using var writer = new StreamWriter(fullPath);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        });

        // Write header manually since we are writing anonymous-like projection
        csv.WriteHeader<ExportedProduct>();
        csv.NextRecord();

        foreach (var category in groupedProducts)
        {
            foreach (var product in category.Value)
            {
                var export = new ExportedProduct
                {
                    Category = category.Key,
                    Id = product.Id,
                    Title = product.Title,
                    OriginalPrice = product.Price,
                    DiscountedPrice = product.DiscountedPrice,
                    Stock = product.Stock,
                    PopularityScore = product.PopularityScore
                };

                csv.WriteRecord(export);
                csv.NextRecord();
            }
        }

        Console.WriteLine($"CSV saved to {fileName}");
    }

    // Define a flat DTO for CSV export
    private class ExportedProduct
    {
        public string Category { get; set; } = "";
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public double OriginalPrice { get; set; }
        public double DiscountedPrice { get; set; }
        public int Stock { get; set; }
        public int PopularityScore { get; set; }
    }
}
