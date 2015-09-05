using System.Web.Mvc;
using ServiceStack.Data;

namespace Ricettario.Controllers
{
    [RoutePrefix("Mobile")]
    public class MobileController : Controller
    {
        private readonly IDbConnectionFactory _dbFactory;

        public MobileController(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        [Route("")]
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
    }
}