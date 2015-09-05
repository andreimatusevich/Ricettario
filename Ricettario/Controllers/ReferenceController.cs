using System;
using System.Linq;
using System.Web.Mvc;
using Ricettario;
using Ricettario.Core.Accessors;
using ServiceStack.Data;

namespace Ricettario.Controllers
{
    [RoutePrefix("Reference")]
    public class ReferenceController : Controller
    {
        private readonly IDbConnectionFactory _dbFactory;

        public ReferenceController(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        [Route("Tags")]
        [HttpGet]
        public JsonResult Tags()
        {
            using (var db = new FluentQuery(_dbFactory.OpenDbConnection()))
            {
                var tags = db.Select<Recipe>().SelectMany(r => (r.Tags ?? "").Split(new[] {",", ";"}, StringSplitOptions.RemoveEmptyEntries)).Distinct().OrderBy(t => t);
                return Json(tags.ToList(), JsonRequestBehavior.AllowGet);
            }
        }
    }
}