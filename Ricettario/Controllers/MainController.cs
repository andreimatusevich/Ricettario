using System.Web.Mvc;
using Ricettario.Controllers.Abstract;
using Ricettario.Core.Accessors;
using ServiceStack.Data;

namespace Ricettario.Controllers
{
    [RoutePrefix("")]
    public class MainController : BaseController
    {
        private readonly IDbConnectionFactory _dbFactory;

        public MainController(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        [Route("Schedule")]
        [HttpGet]
        public ActionResult Schedule()
        {
            return View();
        }

        [Route("Products")]
        [HttpGet]
        public ActionResult Products(int? id, string name)
        {
            return View(new { id, name }.ToExpando());
        }

        [Route("Recipes")]
        [HttpGet]
        public ActionResult Recipes(int? id, string name)
        {
            return View(new { id, name }.ToExpando());
        }

        [Route("Stores")]
        [HttpGet]
        public ActionResult Stores()
        {
            return View();
        }

        [Route("PrintShoppingList/{weekNumber}/{store}/{type?}")]
        [HttpGet]
        public ActionResult PrintShoppingList(int weekNumber, string store, string type = "html")
        {
            var number = Extension.GetNextWeekNumber(weekNumber);
            using (var db = new FluentQuery(_dbFactory.OpenDbConnection()))
            {
                var accessor = new ShoppingAccessor(db);
                var shoppingList = accessor.GetShoppingListForStore(number, store);
                return type == "json" ? JsonCamelCase(shoppingList) : (ActionResult)View(shoppingList);
            }
        }

        [Route("PrintSchedule/{weekNumber}")]
        [HttpGet]
        public ActionResult PrintSchedule(int weekNumber)
        {
            var factory = new ScheduleEmailModelFactory(_dbFactory);
            return View(factory.Get(weekNumber));
        }

        [Route("ShoppingList")]
        [HttpGet]
        public ActionResult ShoppingList(int weekNumber, bool update = false)
        {
            using (var db = new FluentQuery(_dbFactory.OpenDbConnection()))
            {
                var shoppingAccessor = new ShoppingAccessor(db);
                var shoppingList = update ? shoppingAccessor.Update(weekNumber) : shoppingAccessor.GetOrCreate(weekNumber);
                var model = new
                {
                    parentId = shoppingList.Id,
                    parentName = shoppingList.WeekNumber.ToWeekName(),
                    weekNumber = shoppingList.WeekNumber,
                    //published = shoppingList.Published
                }.ToExpando();
                return View(model);
            }
        }
        
        [Route("Departments")]
        [HttpGet]
        public ActionResult Departments(int parentId)
        {
            using (var db = new FluentQuery(_dbFactory.OpenDbConnection()))
            {
                var parent = db.Single<Store>(s => s.Id == parentId);
                return View(new { parentId = parent.Id, parentName = parent.Name }.ToExpando());
            }
        }

        [Route("Ingredients")]
        [HttpGet]
        public ActionResult Ingredients(int parentId)
        {
            using (var db = new FluentQuery(_dbFactory.OpenDbConnection()))
            {
                var parent = db.Single<Recipe>(s => s.Id == parentId);
                return View(new { parentId = parent.Id, parentName = parent.Name }.ToExpando());
            }
        }
    }
}