using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.Linq;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Ricettario.Core.Accessors
{
    public class NewRecipe : Recipe
    {
        public new string Ingredients { get; set; }
    }

    public class RecipeAccessor : EntityAccessor<Recipe>
    {
        public RecipeAccessor(IDbConnectionFactory factory) : base(factory)
        {
        }

        public RecipeAccessor(IDbConnection connection) : base(connection)
        {
        }

        public Recipe CreateNewRecipe(NewRecipe notParsedRecipe)
        {
            var entity = new Recipe();
            entity.Name = notParsedRecipe.Name;
            entity.Reference = notParsedRecipe.Reference;
            entity.Description = notParsedRecipe.Description;
            if (!String.IsNullOrWhiteSpace(notParsedRecipe.Tags))
            {
                entity.Tags = String.Join(",", notParsedRecipe.Tags.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()));
            }

            if (!String.IsNullOrWhiteSpace(notParsedRecipe.Ingredients))
            {
                using (var db = GetConnection())
                {
                    var products = db.Select<Product>().Where(p => p.Name != "").ToList();
                    entity.Ingredients = ParseIngridients(notParsedRecipe.Ingredients, products).ToList();
                }
            }

            return entity;
        }

        public IEnumerable<Ingredient> ParseIngridients(string text, List<Product> products)
        {
            var i = 0;
            if (String.IsNullOrEmpty(text))
            {
                yield return new Ingredient();
            }
            else
            {
                var lines =
                    text.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Where(s => !String.IsNullOrEmpty(s));
                if (lines.Any())
                {
                    var first = lines.First().ToLower();
                    if (first.StartsWith("ingredients"))
                    {
                        lines = lines.Skip(1);
                    }
                }
                var pluralization = PluralizationService.CreateService(CultureInfo.CreateSpecificCulture("en"));
                foreach (var line in lines)
                {
                    var product = ResolveProduct(products, line, pluralization);
                    yield return new Ingredient() { Description = line, Id = i++, ProductId = product.Id, Name = product.Name };
                }
            }
        }

        private Product ResolveProduct(List<Product> products, string rawLine, PluralizationService pluralization)
        {
            var line = rawLine.ToLowerInvariant();
            var lineSplitted = line.Split(' ');
            if (lineSplitted.Contains("egg"))
            {
                line = line.Replace("egg", "chicken egg");
            }
            if (lineSplitted.Contains("eggs"))
            {
                line = line.Replace("eggs", "chicken egg");
            }
            if (line.Contains("black pepper"))
            {
                line = "black pepper in a mill";
            }
            if (line.IsCyrillic())
            {
                line += " " + line.TranslateText("ru", "en");
            }

            var found = products.Where(p => p.Name != null).Where(p => line.Contains(p.Name.ToLower())).OrderByDescending(p => p.Name.Length).ToList();
            if (!found.Any())
            {
                found = products.Where(p => p.Synonyms != null && p.Synonyms.ToLower().Contains(line)).ToList();
                if (!found.Any())
                {
                    found = products.Where(p =>
                    {
                        var n = p.Name.ToLower();
                        n = pluralization.IsPlural(n) ? pluralization.Singularize(n) : pluralization.Pluralize(n);
                        return line.Contains(n);
                    }).OrderByDescending(p => p.Name.Length).ToList();
                }
            }
            return found.Any() ? found.First() : new Product();
        }
    }
}