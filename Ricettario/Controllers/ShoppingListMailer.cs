using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Ricettario.Core.Accessors;
using ServiceStack.Data;

namespace Ricettario.Controllers
{
    public class ShoppingListMailer
    {
        public const string PrintShoppingList = "PrintShoppingList";
        public const string PrintSchedule = "PrintSchedule";


        private readonly Mailer _mailer;
        private readonly IDbConnectionFactory _dbFactory;

        public ShoppingListMailer(Mailer mailer, IDbConnectionFactory dbFactory)
        {
            _mailer = mailer;
            _dbFactory = dbFactory;

            //var printShoppingListPath = HttpContext.Current.Server.MapPath("~/Views/Main/PrintShoppingList.cshtml");
            //Engine.Razor.AddTemplate(PrintShoppingList, new LoadedTemplateSource(File.ReadAllText(printShoppingListPath)));
            //Engine.Razor.Compile(PrintShoppingList);

            //var printSchedulePath = HttpContext.Current.Server.MapPath("~/Views/Main/PrintSchedule.cshtml");
            //Engine.Razor.AddTemplate(PrintSchedule, new LoadedTemplateSource(File.ReadAllText(printSchedulePath)));
            //Engine.Razor.Compile(PrintSchedule);
        }

        public void SendEmails(int weekNumber)
        {
            var weekName = weekNumber.ToWeekName();

            var bjs = CreateShoppingListEmail(weekNumber, "BJs");
            var shoprite = CreateShoppingListEmail(weekNumber, "ShopRite");
            var menu = CreateScheduleEmail(weekNumber);

            _mailer.Email("BJs - Week of " + weekName, bjs, "andrei.matusevich@gmail.com");
            _mailer.Email("ShopRite - Week of " + weekName, shoprite, "andrei.matusevich@gmail.com");
            _mailer.Email("Menu - Week of " + weekName, menu, "andrei.matusevich@gmail.com", "diana.matusevich@gmail.com");//, "tania.a.matusevich@gmail.com");//"aleklimovich@gmail.com");
            //_mailer.Email("Test Menu - Week of " + weekName, menu, "andrei.matusevich@gmail.com");
        }

        private string CreateShoppingListEmail(int weekNumber, string store)
        {
            using (var db = new FluentQuery(_dbFactory.OpenDbConnection()))
            {
                var accessor = new ShoppingAccessor(db);
                var shoppingList = accessor.GetShoppingListForStore(weekNumber, store);

                //using (var templater = new IsolatedRazor.RazorTemplater(null))
                //{
                //    string result = templater.ParseAsync("MyTemplate", "<div>Hello @Model.Name! It is @DateTime.Now</div>", DateTime.Now, new { Name = "ModelTest" }).Result;
                //}
                //~/Static/PrintShoppingList.cshtml
                //"~/Views/Main/PrintShoppingList.cshtml"
                var templatePath = HttpContext.Current.Server.MapPath("~/Static/PrintShoppingList.cshtml");
                //var templatePath = HttpContext.Current.Server.MapPath("~/Views/Main/PrintShoppingList.cshtml");
                using (var templater = new IsolatedRazor.RazorTemplater(templatePath))
                {
                    var emailContent = templater.ParseAsync(PrintShoppingList, File.ReadAllText(templatePath), DateTime.Now, shoppingList).Result;
                    return emailContent;
                }
            }
        }

        public string CreateScheduleEmail(int weekNumber)
        {
            var factory = new ScheduleEmailModelFactory(_dbFactory);
            var templatePath = HttpContext.Current.Server.MapPath("~/Static/PrintSchedule.cshtml");
            //var templatePath = HttpContext.Current.Server.MapPath("~/Views/Main/PrintSchedule.cshtml");
            using (var templater = new IsolatedRazor.RazorTemplater(templatePath))
            {
                var emailContent = templater.ParseAsync(PrintSchedule, File.ReadAllText(templatePath), DateTime.Now, factory.Get(weekNumber)).Result;
                return emailContent;
            }

            //var configuration = new TemplateServiceConfiguration()
            //{
            //    // setting up our custom template manager so we map files on demand
            //    TemplateManager = new SimpleTemplateManager()
            //};
            //var service = RazorEngineService.Create(configuration);

            //var template = File.ReadAllText(HttpContext.Current.Server.MapPath("~/Views/Main/PrintSchedule.cshtml"));
            //var model = new MainController.PrintScheduleViewModel() {Schedule = schedule, SelectedRecipes = selectedRecipes};
            //var emailContent = service.RunCompile(template, PrintSchedule, model.GetType(), model);
            ////var emailContent = Engine.Razor.Run(PrintSchedule, null, new MainController.PrintScheduleViewModel() {Schedule = schedule, SelectedRecipes = selectedRecipes});
            //return emailContent;

        }


    }
}