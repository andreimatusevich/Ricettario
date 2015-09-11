using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using Ricettario.Controllers.Abstract;
using Ricettario.Core.Accessors;
using Ricettario.Models;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Ricettario.Controllers
{
    [RoutePrefix("Store")]
    public class StoreController : EntityController<Store>
    {
        public StoreController(IDbConnectionFactory dbFactory)
            : base(new EntityAccessor<Store>(dbFactory))
        {
        }
    }

    [RoutePrefix("Department")]
    public class DepartmentController : EntityWithParentController<Store, Department>
    {
        public DepartmentController(IDbConnectionFactory dbFactory)
            : base(new EntityAccessor<Store>(dbFactory))
        {
        }
        
        private DepartmentAccessor DepartmentAccessor()
        {
            return Accessor as DepartmentAccessor;
        }

        [HttpDelete]
        [Route("Delete")]
        public override ActionResult Delete(int parentId, Department entity)
        {
            var accessor = DepartmentAccessor();
            accessor.Delete(parentId, entity.Id);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }

    [RoutePrefix("Product")]
    public class ProductController : EntityController<Product>
    {
        public ProductController(IDbConnectionFactory dbFactory)
            : base(new ProductAccessor(dbFactory))
        {
        }

        protected override IEnumerable<Product> Filter(Product entity, List<Product> list)
        {
            var filtered = new FilteredList<Product>(base.Filter(entity, list));
            filtered.Where(entity.Synonyms, item => item.Synonyms != null && item.Synonyms.ToLower().Contains(entity.Synonyms.ToLower()));
            if (entity.WhereToBuy != null && !entity.WhereToBuy.Equals(Location.Empty) && entity.WhereToBuy.StoreId > 0)
            {
                filtered.Where(entity.WhereToBuy, item => item.WhereToBuy != null && item.WhereToBuy.StoreId == entity.WhereToBuy.StoreId);
            }
            filtered.Where(entity.Buy, item => item.Buy == entity.Buy);
            filtered.Where(entity.Skip, item => item.Skip == entity.Skip);
            return filtered.ToEnumerable();
        }
    }

    [RoutePrefix("Recipe")]
    public class RecipeController : EntityController<Recipe>
    {
        public RecipeController(IDbConnectionFactory dbFactory)
            : base(new RecipeAccessor(dbFactory))
        {
        }

        private RecipeAccessor RecipeAccessor()
        {
            return Accessor as RecipeAccessor;
        }

        [Route("Add")]
        [HttpPost]
        public void AddRecipe(NewRecipe newRecipe)
        {
            var accessor = RecipeAccessor();
            var recipe = accessor.CreateNewRecipe(newRecipe);
            accessor.Post(recipe);
        }

        protected override IEnumerable<Recipe> Filter(Recipe entity, List<Recipe> list)
        {
            var filtered = new FilteredList<Recipe>(base.Filter(entity, list));
            filtered.Where(entity.Reference, item =>
            {
                var text = entity.Reference.ToLower();
                var result = false;
                if (item.Description != null)
                {
                    result = result || item.Description.ToLower().Contains(text);
                }
                if (item.Reference != null)
                {
                    result = result || item.Reference.ToLower().Contains(text);
                }
                return result;
            });
            filtered.Where(entity.Tags, item => item.Tags.ToLower().Contains(entity.Tags.ToLower()));
            return filtered.ToEnumerable();
        }
    }
    
    [RoutePrefix("Ingredient")]
    public class IngredientController : EntityWithParentController<Recipe, Ingredient>
    {
        public IngredientController(IDbConnectionFactory dbFactory)
            : base(new RecipeAccessor(dbFactory))
        {
        }

        protected override IEnumerable<Ingredient> Filter(Ingredient entity, List<Ingredient> list)
        {
            var filtered = new FilteredList<Ingredient>(base.Filter(entity, list));
            filtered.Where(entity.Description, item => item.Description.ToLower().Contains(entity.Description.ToLower()));
            filtered.Where(entity.ProductId, item => item.ProductId == entity.ProductId);
            return filtered.ToEnumerable();
        }
    }
    
    [RoutePrefix("Api/ShoppingList")]
    public class ShoppingListController : EntityController<ShoppingList>
    {
        private readonly IDbConnectionFactory _dbFactory;

        public ShoppingListController(IDbConnectionFactory dbFactory)
            : base(new EntityAccessor<ShoppingList>(dbFactory))
        {
            _dbFactory = dbFactory;
        }

        [Route("{weekNumber?}")]
        [HttpGet]
        public JsonResult ShoppingList(int? weekNumber)
        {
            var number = Extension.GetNextWeekNumber(weekNumber);
            using (var db = new FluentQuery(_dbFactory.OpenDbConnection()))
            {
                var accessor = new ShoppingAccessor(db);
                var shoppingList = accessor.GetSingle(number);
                var groupedAndSorted = shoppingList.Items
                    .Where(i => i.Buy)
                    .OrderBy(i => i.Store)
                    .ThenBy(i => i.Department)
                    .ThenBy(i => i.OrderHelper)
                    .ThenBy(i => i.Product)
                    .GroupBy(i => (i.Store == "ShopRite" || i.Store == "BJs") ? i.Store : "Other");

                var viewModel = new ShoppingListViewModel()
                {
                    Id = shoppingList.Id,
                    Name = shoppingList.Name,
                    WeekNumber = shoppingList.WeekNumber,
                    Stores = groupedAndSorted.Select(g => new ShoppingListViewModel.Store() {Name = g.Key, Items = g.ToList()}).ToList()
                };

                return JsonCamelCase(viewModel);
            }
        }
    }
    
    [RoutePrefix("ShoppingListItem")]
    public class ShoppingListItemController : EntityWithParentController<ShoppingList, ShoppingListItem>
    {
        public ShoppingListItemController(IDbConnectionFactory dbFactory)
            : base(new EntityAccessor<ShoppingList>(dbFactory))
        {
        }

        protected override IEnumerable<ShoppingListItem> OrderBy(IEnumerable<ShoppingListItem> array)
        {
            return array.OrderByDescending(i => i.Buy).ThenBy(i => i.Product);
        }

        protected override IEnumerable<ShoppingListItem> Filter(ShoppingListItem entity, List<ShoppingListItem> list)
        {
            var filtered = new FilteredList<ShoppingListItem>(base.Filter(entity, list));
            filtered.Where(entity.Store, item => item.Store.ToLower().Contains(entity.Store.ToLower()));
            filtered.Where(entity.Department, item => item.Department.ToLower().Contains(entity.Department.ToLower()));
            filtered.Where(entity.Recipe, item => item.Store.ToLower().Contains(entity.Recipe.ToLower()));
            filtered.Where(entity.Product, item => item.Store.ToLower().Contains(entity.Product.ToLower()));
            filtered.Where(entity.Buy, item => item.Buy == entity.Buy);
            filtered.Where(entity.OrderHelper, item => item.OrderHelper.ToLower().Contains(entity.OrderHelper.ToLower()));
            return filtered.ToEnumerable();
        }
    }

    [RoutePrefix("Schedule")]
    public class WeekScheduleController : EntityController<WeekSchedule>
    {
        private readonly IDbConnectionFactory _dbFactory;

        public WeekScheduleController(IDbConnectionFactory dbFactory)
            : base(new EntityAccessor<WeekSchedule>(dbFactory))
        {
            _dbFactory = dbFactory;
            OrderByFunc = e => e.Id.ToString(CultureInfo.InvariantCulture);
        }

        [Route("WeekData")]
        [HttpGet]
        public JsonResult WeekData(int? weekNumber)
        {
            WeekSchedule week;
            var number = Extension.GetNextWeekNumber(weekNumber);
            using (var db = new FluentQuery(_dbFactory.OpenDbConnection()))
            {
                var accessor = new WeekAccessor(db);
                week = accessor.GetOrCreate(number);
            }

            var previousWeekNumber = week.Date.AddDays(-7).ToWeekNumber();
            var nextWeekNumber = week.Date.AddDays(+7).ToWeekNumber();
            var result = new
            {
                week,
                previous = new { weekNumber = previousWeekNumber, weekName = previousWeekNumber.ToWeekName() },
                next = new { weekNumber = nextWeekNumber, weekName = nextWeekNumber.ToWeekName() }
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Route("SendEmails")]
        [HttpPost]
        public void SendEmails(int weekNumber)
        {
            var mailer = new Mailer();
            var shoppingListMailer = new ShoppingListMailer(mailer, _dbFactory);
            shoppingListMailer.SendEmails(weekNumber);

            using (var db = _dbFactory.OpenDbConnection())
            {
                var list = db.Single<ShoppingList>(sl => sl.WeekNumber == weekNumber);
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
                    mailer.Email("Others - Week of " + weekNumber.ToWeekName(), sb.ToString(), "andrei.matusevich@gmail.com");
                }
            }
        }

        [Route("AddProduct")]
        [HttpPost]
        public void AddProduct(int weekNumber, int id, string name)
        {
            using (var db = new FluentQuery(_dbFactory.OpenDbConnection()))
            {
                var accessor = new WeekAccessor(db);
                accessor.AddProduct(weekNumber, new NamedReference() { Id = id, Name = name });
            }
        }

        [Route("AddRecipes")]
        [HttpPost]
        public void AddRecipesToMeal(int weekNumber, int day, int meal, int[] recipes)
        {
            using (var db = new FluentQuery(_dbFactory.OpenDbConnection()))
            {
                var accessor = new WeekAccessor(db);
                accessor.AddRecipes(weekNumber, day, meal, recipes);
            }
        }

        [Route("RemoveRecipe")]
        [HttpPost]
        public void RemoveRecipeFromMeal(int weekNumber, int day, int meal, int recipe)
        {
            using (var db = new FluentQuery(_dbFactory.OpenDbConnection()))
            {
                var accessor = new WeekAccessor(db);
                accessor.DeleteMeal(new GlobalEntryReference() {WeekNumber = weekNumber, DayIndex = day, MealIndex = meal, RecipeId = recipe});
            }
        }
    }
}