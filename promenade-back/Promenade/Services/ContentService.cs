using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Promenade.Models;

namespace Promenade.Services
{
    public class ContentService
    {
        private readonly ILogger<ContentService> _logger;
        private Category[] _categories;

        public ContentService(ILogger<ContentService> logger)
        {
            _logger = logger;
        }

        public void Init()
        {
            // read categories file
            var categoriesFile = File.ReadAllText("poi.json");
            var categories = JsonConvert.DeserializeObject<Category[]>(categoriesFile, Utils.ConverterSettings);
            _categories = categories ?? throw new Exception("No categories found");

            _logger.LogInformation(
                $"Categories loaded: {_categories.Length}; poi: {_categories.Select(c => c.Tags.Length).Sum()}"
            );
        }
    }
}