using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using Ricettario;

namespace Tests
{
    public class ShoppingListTests
    {
        [Test]
        public void Prepare()
        {
            var schedule = JsonConvert.DeserializeObject<WeekSchedule>(File.ReadAllText(@"..\..\..\TestData\weekschedule.txt"));
            var recipes = JsonConvert.DeserializeObject<List<Recipe>>(File.ReadAllText(@"..\..\..\CsvTest.Recipes.approved.txt"));
            var products = JsonConvert.DeserializeObject<List<Product>>(File.ReadAllText(@"..\..\..\CsvTest.Products.approved.txt"));
            var stores = JsonConvert.DeserializeObject<List<Store>>(File.ReadAllText(@"..\..\..\CsvTest.Stores.approved.txt"));

            //var items = ShoppingService.GetShoppingListItems(schedule, stores, recipes, products);

            //var sorted = items.OrderByDescending(i => i.Buy).ThenBy(i => i.Product);

            //sorted = items.Where(i => i.Buy).OrderBy(i => i.Store).ThenBy(i => i.OrderHelper).ThenBy(i => i.Product);

            //foreach (var item in sorted)
            //{
            //    Console.WriteLine(item.Store + ";" + item.Department + ";" + item.Recipe + ";" + item.Description + ";" + item.Product + ";" + item.Buy);
            //}
        }
    }

}