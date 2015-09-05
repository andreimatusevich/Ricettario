using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Web;
using ApprovalTests;
using ApprovalTests.Reporters;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using Ricettario;
using Tests.Import;

namespace Tests
{
    public class CsvTest
    {
        //issues
        //Lemon muffins - products assignment

        [Test]
        public void TestTranslate()
        {
            var russian = "мёд";
            var english = "honey";
            
            var translated = russian.TranslateText("ru", "en");
            translated.Should().Be(english);

            russian.IsCyrillic().Should().BeTrue();
            english.IsCyrillic().Should().BeFalse();
            "1/4".IsCyrillic().Should().BeFalse();
        }

        public static string GetPath(string fileName)
        {
            return @"..\..\..\" + fileName;
        }

        [Test]
        [UseReporter(typeof (WinMergeReporter))]
        public void Recipes()
        {
            var imported = GetImportedRecipes();
            var types = GetImportedRecipeTypes();
            var products = JsonConvert.DeserializeObject<List<Product>>(File.ReadAllText(GetPath("CsvTest.Products.approved.txt")));
            var recipes = GetRecipes(imported, types, products).Distinct(new DistinctRecipeComparer()).OrderBy(r => r.Name).Where(r => r.Ingredients.Any(i => i.ProductId != -1)).ToList();
            var n = 0;
            foreach (var recipe in recipes)
            {
                recipe.Id = n++;
            }
            
            var json = JsonConvert.SerializeObject(recipes, Formatting.Indented);
            var path = Environment.CurrentDirectory;
            Approvals.Verify(json);

            Console.WriteLine("Not referenced:");
            foreach (var r in recipes.Where(r => !r.Tags.Any()))
            {
                Console.WriteLine(r.Name);
            }

            Console.WriteLine();
            Console.WriteLine("Рецепт не расписан:");
            foreach (var r in recipes.Where(r => r.Ingredients[0].Description == "Рецепт не расписан"))
            {
                Console.WriteLine(r.Name);
            }
            
            Console.WriteLine();
            Console.WriteLine("Product is not set:");
            foreach (var r in recipes.Where(r => r.Name != r.Ingredients[0].Description && r.Ingredients[0].Description != "Рецепт не расписан" && r.Ingredients.Any(i => i.ProductId == 0)))
            {
                Console.WriteLine(r.Name);
            }

            var ingredients = recipes.SelectMany(r => r.Ingredients).ToList();
            Console.WriteLine();
            Console.WriteLine("Product is not used:");
            foreach (var product in products)
            {
                if (ingredients.All(i => i.ProductId != product.Id))
                {
                    Console.WriteLine(product.Id + " " + product.Name + " " + product.Synonyms);
                }
            }
            
            TestDeserialize<Recipe>(GetPath("CsvTest.Recipes.approved.txt"));
        }

        [Test]
        [UseReporter(typeof (WinMergeReporter))]
        public void Stores()
        {
            var products = GetImportedProducts();

            var stores = GetStores(products);

            var json = JsonConvert.SerializeObject(stores, Formatting.Indented);
            Approvals.Verify(json);

            TestDeserialize<Store>(GetPath("CsvTest.Stores.approved.txt"));
        }

        [Test]
        [UseReporter(typeof (WinMergeReporter))]
        public void Products()
        {
            var productsConverted = GetProducts().OrderBy(p => p.Name);
            var n = 0;
            foreach (var recipe in productsConverted)
            {
                recipe.Id = n++;
            }

            var json = JsonConvert.SerializeObject(productsConverted, Formatting.Indented);
            Approvals.Verify(json);

            TestDeserialize<Product>(GetPath("CsvTest.Products.approved.txt"));
        }

        private static void TestDeserialize<T>(string path)
        {
            var deserialized = JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(path));
            var json = JsonConvert.SerializeObject(deserialized, Formatting.Indented);
            Approvals.Verify(json);
        }

        private IEnumerable<Recipe> GetRecipes(IEnumerable<ImportedRecipe> importedRecipes, List<ImportedRecipeType> types, List<Product> products)
        {
            var id = 0;
            yield return
                new Recipe()
                {
                    Id = id,
                    Name = "",
                    Description = "",
                    Reference = "",
                    Ingredients = new List<Ingredient>() {new Ingredient() {Description = "", ProductId = 0, Id = 0}},
                    Tags = ""
                };

            Recipe recipe = null;
            foreach (var r in importedRecipes)
            {
                if (r.Entry == "hhhhh")
                {
                    yield return recipe;
                    yield break;
                }
                if (!String.IsNullOrWhiteSpace(r.Entry))
                {
                    if (r.Entry.ToLower() == r.Ingredients.ToLower() && r.Ingredients.ToLower() == r.Product.ToLower())
                    {
                        continue;
                    }
                    if (recipe != null)
                    {
                        yield return recipe;
                    }
                    recipe = new Recipe();
                    recipe.Id = ++id;
                    recipe.Name = r.Entry;
                    recipe.Reference = ToReference(r);
                    recipe.Tags = String.Join(",", types.Where(t => t.Product.ToLower() == r.Entry.ToLower()).Select(t => t.Type).Distinct().OrderBy(s => s).ToList());
                    PopulateIngredients(recipe, r, products);
                }
                else
                {
                    if (!String.IsNullOrWhiteSpace(r.Product))
                    {
                        PopulateIngredients(recipe, r, products);
                    }
                }
            }
        }

        private static void PopulateIngredients(Recipe recipe, ImportedRecipe imported, IEnumerable<Product> products)
        {
            //if (String.IsNullOrWhiteSpace(r.Product) && r.Ingredients != "Рецепт не расписан") throw new ArgumentException("Product is empty " + r.Ingredients);
            var length = recipe.Ingredients.Count();

            var name = imported.Product.ToLower();
            if (name == "Макароны".ToLower())
            {
                name = "pasta";
            }
            if (name == "разрыхлитель теста")
            {
                name = "baking powder";
            }

            if (name == "покупное")
            {
                recipe.Ingredients.Add(new Ingredient() {Description = "покупное", ProductId = -1});
                return;
            }

            var product = products.SingleOrDefault(p => p.Name.ToLower() == name);
            if (product == null)
            {
                product = products.Single(p => p.Synonyms.ToLower().Contains(name.ToLower()));
            }

            recipe.Ingredients.Add(new Ingredient() {Description = imported.Ingredients, ProductId = product.Id, Id = length});
        }

        private string ToReference(ImportedRecipe recipe)
        {
            var sb = new StringBuilder();
            if (!String.IsNullOrWhiteSpace(recipe.Pagano))
            {
                sb.AppendLine("Pagano: " + recipe.Pagano);
            }
            if (!String.IsNullOrWhiteSpace(recipe.DamianBook))
            {
                sb.AppendLine("For Damian: " + recipe.DamianBook);
            }
            if (!String.IsNullOrWhiteSpace(recipe.Receptishi))
            {
                sb.AppendLine("Belonika receptishi: " + recipe.Receptishi);
            }
            if (!String.IsNullOrWhiteSpace(recipe.Dietishi))
            {
                sb.AppendLine("Belonika dietishi: " + recipe.Dietishi);
            }
            if (!String.IsNullOrWhiteSpace(recipe.Internet))
            {
                sb.AppendLine(recipe.Internet);
            }
            return sb.ToString();
        }

        private static IEnumerable<Store> GetStores(IEnumerable<ImportedProduct> products)
        {
            var stores = (new[] { ImportedProduct.Empty }).Concat(products).Select(p => new { Store = p.Store.FixStore(), p.Department })
                .GroupBy(i => i.Store).OrderBy(g => g.Key)
                .Select(
                    (g, index1) =>
                        new Store()
                        {
                            Id = index1,
                            Name = g.Key,
                            Departments =
                                g.Select(v => v.Department).Distinct().OrderBy(s => s).Select((d, index2) => new Department() {Id = index2, Name = d}).ToList()
                        });
            return stores;
        }

        private static IEnumerable<Product> GetProducts()
        {
            var products = GetImportedProducts();

            var stores = GetStores(products);

            var productsConverted =
                (new[] {ImportedProduct.Empty}).Concat(products)
                    .Distinct(new DistinctProductComparer()).OrderBy(p => p.Product)
                    .Select((p, index) =>
                    {
                        var store = stores.Single(s => s.Name == p.Store.FixStore());
                        var department = store.Departments.Single(d => d.Name == p.Department);
                        return new Product()
                        {
                            Id = index,
                            Name = p.Product.ToLower(),
                            Buy = p.Buy.ToBool(),
                            Skip = p.Skip.ToBool(),
                            WhereToBuy = new Location() {StoreId = store.Id, DepartmentId = department.Id}
                        };
                    }).ToList();

            var result = new Dictionary<string, Product>();
            foreach (var product in productsConverted)
            {
                if (product.Name.All(c => c <= sbyte.MaxValue))
                {
                    var russian = TranslateFromEnToRu(product.Name);
                    if (String.IsNullOrEmpty(russian))
                    {
                        russian = Extension.TranslateText(product.Name, "en", "ru");
                    }
                    if (!String.IsNullOrEmpty(russian) && !russian.All(c => c <= sbyte.MaxValue))
                    {
                        product.AddSynonym(russian);
                    }
                }
                else
                {
                    var russian = product.Name;
                    product.AddSynonym(russian);
                    var english = TranslateFromRuToEn(russian);
                    if (String.IsNullOrEmpty(english))
                    {
                        english = Extension.TranslateText(russian, "ru", "en");
                    }
                    if (String.IsNullOrEmpty(english))
                    {
                        throw new Exception("No transaltion for " + russian);
                    }
                    product.Name = english;
                }
                if (result.ContainsKey(product.Name))
                {
                    Console.Write("|------------------------------------- ");
                }
                else
                {
                    result.Add(product.Name, product);
                }
                Console.WriteLine(product.Name + " -> " + product);
            }

            return result.Values;
        }

        private static Dictionary<string, string> _ruToEn;

        public static string TranslateFromRuToEn(string text)
        {
            if (_ruToEn == null)
            {
                _ruToEn = File.ReadLines(GetPath(@"TestData\RuToEn.txt")).Select(line => line.Split(',')).ToDictionary(line => line[1].ToLower(), line => line[0]);
            }
            if (_ruToEn.ContainsKey(text.ToLower()))
            {
                return _ruToEn[text.ToLower()].ToLower();
            }
            return null;
        }

        private static Dictionary<string,string> _enToRu ;

        public static string TranslateFromEnToRu(string text)
        {
            if (_enToRu == null)
            {
                _enToRu = File.ReadLines(GetPath(@"TestData\EnToRu.txt")).Select(line => line.Split(',')).ToDictionary(line => line[0].ToLower(), line => line[1]);
            }
            if (_enToRu.ContainsKey(text.ToLower()))
            {
                return _enToRu[text.ToLower()].ToLower();
            }
            return null;
        }

        class DistinctProductComparer : IEqualityComparer<ImportedProduct>
        {
            public bool Equals(ImportedProduct x, ImportedProduct y)
            {
                return x.Product == y.Product;
            }

            public int GetHashCode(ImportedProduct obj)
            {
                return obj.Product.GetHashCode();
            }
        }

        class DistinctRecipeComparer : IEqualityComparer<Recipe>
        {
            public bool Equals(Recipe x, Recipe y)
            {
                return x.Name == y.Name;
            }

            public int GetHashCode(Recipe obj)
            {
                return obj.Name.GetHashCode();
            }
        }

        private static List<ImportedProduct> GetImportedProducts()
        {
            var text = File.ReadAllText(@"..\..\..\Import\Data\Products.csv");
            var csv = new CsvHelper.CsvReader(new StreamReader(text.ToStream()));
            csv.Configuration.IsHeaderCaseSensitive = false;
            csv.Configuration.TrimFields = true;
            var products = csv.GetRecords<ImportedProduct>().ToList();
            //products.Count().Should().Be(217);
            return products;
        }
        
        private static List<ImportedRecipe> GetImportedRecipes()
        {
            var text = File.ReadAllText(@"..\..\..\Import\Data\Recipes.csv");
            var csv = new CsvHelper.CsvReader(new StreamReader(text.ToStream()));
            csv.Configuration.IsHeaderCaseSensitive = false;
            csv.Configuration.TrimFields = true;
            var result = csv.GetRecords<ImportedRecipe>().ToList();
            //result.Count().Should().Be(960);
            return result;
        }

        private static List<ImportedRecipeType> GetImportedRecipeTypes()
        {
            var text = File.ReadAllText(@"..\..\..\Import\Data\Types.csv");
            var csv = new CsvHelper.CsvReader(new StreamReader(text.ToStream()));
            csv.Configuration.IsHeaderCaseSensitive = false;
            csv.Configuration.TrimFields = true;
            var result = csv.GetRecords<ImportedRecipeType>().ToList();
            //result.Count().Should().Be(194);
            return result;
        }
    }


    public static class Extensions
    {
        public static bool ToBool(this string text)
        {
            return text == "1";
        }

        public static string FixStore(this string text)
        {
            if (text == "Bjs") return "BJs";
            if (text == "Shoprite") return "ShopRite";
            return text;
        }

        public static MemoryStream ToStream(this string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));
        }

        public static void WriteAll(params object[] parameters)
        {
            Console.WriteLine(parameters.Aggregate((current, next) => current + ", " + next));
        }
    }
}
