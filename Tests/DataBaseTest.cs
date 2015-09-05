using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;
using Ricettario;
using Ricettario.Core.Accessors;
using ServiceStack.OrmLite;

namespace Tests
{
    public class DataBaseTest
    {
        const string SqliteFileDb = @"d:\work\RicettarioWeb\RazorRockstars.SelfHost\bin\App_Data\db.sqlite";

        [Test]
        public void ShoppingListRecipes()
        {
            var factory = new OrmLiteConnectionFactory(SqliteFileDb, SqliteDialect.Provider);
            using (var db = factory.OpenDbConnection())
            {
                var list = db.Single<ShoppingList>(sl => sl.WeekNumber == 20150223);
                var otherItems = list.Items.Where(i => i.Store != null && i.Store != "BJs" && i.Store != "ShopRite" && i.Buy);
                if (otherItems.Any())
                {
                    var sb = new StringBuilder();
                    foreach (var group in otherItems.GroupBy(i => i.Store))
                    {
                        sb.AppendLine(group.First().Store + ":");
                        foreach (var item in group)
                        {
                            sb.AppendLine(item.Name);
                        }
                        sb.AppendLine();
                    }
                    Console.WriteLine(sb.ToString());
                }
            }
        }

        [Test]
        public void LoadRecipes()
        {
            var factory = new OrmLiteConnectionFactory(SqliteFileDb, SqliteDialect.Provider);
            using (var db = factory.OpenDbConnection())
            {
                var list = db.Select<Recipe>().ToList();

                //foreach (var entry in list.Where(r => r.Ingredients.Any(i => i.Description != null && i.Description.Contains("salmon fillet"))))
                foreach (var entry in list.Where(r => r.Ingredients.Any(i => i.ProductId == 212)))
                {
                    Console.WriteLine(entry.Id + " " + entry.Name);
                }
            }
        }

        [Test]
        public void LoadProducts()
        {
            var factory = new OrmLiteConnectionFactory(SqliteFileDb, SqliteDialect.Provider);
            using (var db = factory.OpenDbConnection())
            {
                var list = db.Select<Product>().ToList();
                //var prodcuts = db.Select<Product>().ToList();

                foreach (var entry in list.Where(p => p.Id == list.Max(p1 => p1.Id)))
                {
                    Console.WriteLine(entry.Name);
                }
            }
        }


        [Test]
        public void Test()
        {
            var factory = new OrmLiteConnectionFactory(SqliteFileDb, SqliteDialect.Provider);
            //const string storedPath = @"d:\work\RicettarioWeb\RazorRockstars.SelfHost\bin\App_Data\WeekSchedules.txt";
            //var stored = JsonConvert.DeserializeObject<List<WeekSchedule>>(File.ReadAllText(storedPath));
            //Console.WriteLine(stored.Count);
            using (var db = factory.OpenDbConnection())
            {
                //db.DropTable<WeekSchedule>();
                //db.CreateTableIfNotExists<WeekSchedule>();
                //foreach (var item in stored)
                //{
                //    db.Insert(item);
                //}

                //var list = shopping.Single(s => s.Id == 14);
                //var depart= list.Items.Single(i => i.Index == 83);
                //Console.WriteLine(depart.Description);
                //depart.Description = depart.Description.Replace("'", "");
                //db.Update(list);

                var stored = db.Select<WeekSchedule>().ToList();
                //var prodcuts = db.Select<Product>().ToList();

                foreach (var weekSchedule in stored)
                {
                    weekSchedule.Name = weekSchedule.Id.ToWeekName();
                    db.Update(weekSchedule);
                    //foreach (var product in weekSchedule.Products)
                    //{
                    //    //var lookedup = prodcuts.Single(p => p.Id == product);
                    //    //weekSchedule.Products2.Add(new NamedReference() { Id = product, Name = lookedup.Name });
                    //}
                }

                //File.WriteAllText(storedPath, JsonConvert.SerializeObject(stored, Formatting.Indented));
                //< List < Product >> (File.ReadAllText(GetPath("CsvTest.Products.approved.txt")));
                //foreach(var product in db.Select<Product>().ToList())
                //{
                //    if (product.WhereToBuy != null && product.WhereToBuy.StoreId == 8 && product.WhereToBuy.DepartmentId == 29)
                //    {
                //        Console.WriteLine(product.Id + ' ' + product.Name);
                //    }
                //}
            }
        }

        [Test]
        public void Insert()
        {
            var factory = new OrmLiteConnectionFactory(SqliteFileDb, SqliteDialect.Provider);
            using (var db = factory.OpenDbConnection())
            {
                db.Insert(new Product() {Id = 0, Buy = false, Skip = false, Synonyms = "", Name = "", WhereToBuy = new Location() {DepartmentId = 0, StoreId = 0}});
            }
        }

        [Test]
        public void ReorderDepartments()
        {
            var factory = new OrmLiteConnectionFactory(SqliteFileDb, SqliteDialect.Provider);
            using (var db = factory.OpenDbConnection())
            {
                foreach (var entity in db.Select<Store>().ToList())
                {
                    entity.Departments = entity.Departments.OrderBy(d => d.Name).ToList();
                    db.Update(entity);
                }
            }
        }

        [Test]
        public void FixArrayConvertedToString()
        {
            var factory = new OrmLiteConnectionFactory(SqliteFileDb, SqliteDialect.Provider);
            using (var db = factory.OpenDbConnection())
            {
                foreach(var entity in db.Select<Recipe>().ToList())
                {
                    entity.Tags = entity.Tags.Replace("[", "").Replace("]", "");
                    db.Update(entity);
                }
            }
            //Delete(new Product() { Id = 281 });
        }

        public void Delete<T>(T entity) where T : IIdentifiable
        {
            var factory = new OrmLiteConnectionFactory(SqliteFileDb, SqliteDialect.Provider);
            using (var db = factory.OpenDbConnection())
            {
                var list = db.Select<T>();
                Console.WriteLine(list.Count);
                var id = entity.Id;
                db.Delete<T>(p => p.Id == id);
                list = db.Select<T>();
                Console.WriteLine(list.Count);
            }
        }
    }
}