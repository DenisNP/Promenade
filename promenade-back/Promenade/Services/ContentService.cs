using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Promenade.Geo;
using Promenade.Models;

namespace Promenade.Services
{
    public class ContentService
    {
        private readonly ILogger<ContentService> _logger;
        private Dictionary<int, Category> _categories;

        public ContentService(ILogger<ContentService> logger)
        {
            _logger = logger;
        }

        public void Init()
        {
            // read categories file
            var categoriesFile = File.ReadAllText("poi.json");
            var categories = JsonConvert.DeserializeObject<Category[]>(categoriesFile, Utils.ConverterSettings);
            _categories = categories?.ToDictionary(c => c.Id) ?? throw new Exception("No categories found");

            _logger.LogInformation(
                $"Categories loaded: {_categories.Count()}; poi: {_categories.Select(c => c.Value.Tags.Length).Sum()}"
            );
        }

        public KeyValuePair<string, string>[] GetTagsForCategories(int[] categoryIds)
        {
            return _categories
                .Values
                .Where(c => categoryIds.Contains(c.Id))
                .SelectMany(c => c.Tags)
                .Select(t => t.ToKvPair())
                .ToArray();
        }

        public int[] GetAllIds()
        {
            return _categories.Keys.ToArray();
        }

        public CategoryForUser[] GenerateInitial()
        {
            return _categories
                .Select(c => new CategoryForUser(c.Value, c.Value.DefaultEnabled))
                .ToArray();
        }

        public void FillEmptyData(Poi poi)
        {
            var (catId, subId) = GetCategoryId(poi, out var tagName);
            poi.CategoryId = catId;
            poi.FullTagId = catId * 100 + subId;

            if (catId >= 0 && string.IsNullOrEmpty(poi.Description))
                poi.Description = tagName;
        }

        private (int categoryId, int subcategoryId) GetCategoryId(Poi poi, out string tagName)
        {
            foreach (var category in _categories.Values)
            {
                for (var i = 0; i < category.Tags.Length; i++)
                {
                    var tagData = category.Tags[i];
                    if (poi.Tags.Any(t => t.Key == tagData.Key && t.Value == tagData.Value))
                    {
                        tagName = tagData.Name.UppercaseFirst();
                        return (category.Id, i);
                    }
                }
            }

            tagName = "";
            return (-1, -1);
        }
    }
}