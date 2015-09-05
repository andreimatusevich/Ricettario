using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack;

namespace Ricettario.Core.Accessors
{
    public class ShoppingAccessor
    {
        private readonly IEasyData _db;

        public ShoppingAccessor(IEasyData db)
        {
            _db = db;
        }

        public void UpdateItem(int weekNumber, ShoppingListItem input)
        {
            var shoppingList = _db.Single<ShoppingList>(q => q.WeekNumber == weekNumber);
            var item = shoppingList.Items.Single(i => i.Id == input.Id);

            if (item.Buy != input.Buy)
            {
                foreach (var i in shoppingList.Items.Where(r => r.Product == item.Product))
                {
                    i.Buy = input.Buy;
                }
            }

            item.PopulateWith(input);
            _db.Update(shoppingList);
        }
        
        public ShoppingList GetShoppingListForStore(int weekNumber, string store)
        {
            var shoppingList = GetSingle(weekNumber);
            var sorted = shoppingList.Items.Where(i => i.Buy && i.Store == store).OrderBy(i => i.Store).ThenBy(i => i.Department).ThenBy(i => i.OrderHelper).ThenBy(i => i.Product).ToList();
            if (!sorted.Any())
            {
                sorted.Add(new ShoppingListItem() { Store = store });
            }
            shoppingList.Items = sorted;
            return shoppingList;
        }

        public void Delete(int weekNumber)
        {
            _db.Delete<ShoppingList>(q => q.WeekNumber == weekNumber);
        }
        
        public ShoppingList GetOrCreate(int weekNumber)
        {
            var week = _db.Single<WeekSchedule>(q => q.Id == weekNumber);
            var shoppingList = _db.Single<ShoppingList>(q => q.WeekNumber == weekNumber);
            if (shoppingList == null)
            {
                shoppingList = new ShoppingList()
                {
                    WeekNumber = weekNumber,
                    Items = CreateShoppingListItems(week, _db.Select<Recipe>(), _db.Select<Product>()).OrderByDescending(i => i.Buy).ThenBy(i => i.Product).ToList()
                };
                shoppingList.Id = (int)_db.Insert(shoppingList);
            }
            return shoppingList;
        }

        public ShoppingList Update(int weekNumber)
        {
            var shoppingList = _db.Single<ShoppingList>(q => q.WeekNumber == weekNumber);
            if (shoppingList == null)
            {
                return GetOrCreate(weekNumber);
            }
            else
            {
                var week = _db.Single<WeekSchedule>(q => q.Id == weekNumber);
                var items = CreateShoppingListItems(week, _db.Select<Recipe>(), _db.Select<Product>()).OrderByDescending(i => i.Buy).ThenBy(i => i.Product).ToList();
                var newItems = items.GroupJoin(shoppingList.Items, item => item.Product, olditem => olditem.Product, (item, gj) => new {item, gj})
                    .SelectMany(@t => @t.gj.DefaultIfEmpty(), (@t, subitem) =>
                    {
                        if (subitem != null)
                        {
                            @t.item.Buy = subitem.Buy;
                        }
                        return @t.item;
                    });

                shoppingList.Items = newItems.ToList();
                _db.Update(shoppingList);
            }

            return shoppingList;
        }

        public ShoppingList GetSingle(int weekNumber)
        {
            var result = _db.Single<ShoppingList>(q => q.WeekNumber == weekNumber);
            if(result != null)
            {
                result.Name = weekNumber.ToWeekName();
            }
            return result;
        }

        private IEnumerable<ShoppingListItem> CreateShoppingListItems(WeekSchedule schedule, IEnumerable<Recipe> recipes, IEnumerable<Product> products)
        {
            var departments = SelectStoreDepartments();

            var recipeIds = schedule.Days.SelectMany(d => d.Meals.SelectMany(m => m.Entries)).Select(rr => rr.RecipeId).Distinct();

            var selectedRecipes = from rId in recipeIds
                join r in recipes on rId equals r.Id
                select r;

            selectedRecipes = selectedRecipes.Concat(new[] { new Recipe() { Name = "To Buy", Ingredients = schedule.Products.Select(p => new Ingredient() { ProductId = p.Id }).ToList() } });

            var items = selectedRecipes.SelectMany(r => (
                from ij in r.Ingredients
                join pj in products on ij.ProductId equals pj.Id
                join dj in departments on new { pj.WhereToBuy.StoreId, pj.WhereToBuy.DepartmentId } equals new { dj.StoreId, dj.DepartmentId }
                select new { ingredient = ij, product = pj, department = dj, recipe = r.Name })).Select(
                    (j, index) =>
                        new ShoppingListItem
                        {
                            Id = index,
                            Recipe = j.recipe,
                            Name = j.ingredient.Description,
                            Product = j.product.Name,
                            Buy = j.recipe == "To Buy" || j.product.Buy,
                            Store = j.department.Store,
                            Department = j.department.Department,
                            OrderHelper = String.IsNullOrWhiteSpace(j.department.Department) ? "Z" : j.department.Department
                        });

            var groupedByProduct = items.GroupBy(i => i.Product).Select(g =>
            {
                var first = g.First();
                var description = g.Select(sl => sl.Recipe + " - " + sl.Name).Aggregate((current, next) => current + Environment.NewLine + next);
                return new ShoppingListItem()
                {
                    Id = first.Id,
                    Store = first.Store,
                    Department = first.Department,
                    Name = description,
                    Product = first.Product,
                    Buy = g.Any(i => i.Buy)
                };
            });
            
            return groupedByProduct.ToList();
        }

        private IEnumerable<StoreDepartment> SelectStoreDepartments()
        {
            var stores =
                _db.Select<Store>()
                    .SelectMany(
                        s =>
                            s.Departments.Select(
                                d =>
                                    new StoreDepartment
                                    {
                                        StoreId = s.Id,
                                        Store = s.Name,
                                        DepartmentId = d.Id,
                                        Department = d.Name
                                    }));
            return stores;
        }
    }
}