using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Ricettario;
using Ricettario.Core.Accessors;

namespace Ricettario.Controllers.Abstract
{
    public abstract class EntityController<T> : BaseController where T : IIdentifiable, new()
    {
        protected readonly IEntityAccessor<T> Accessor;
        protected Func<T, string> OrderByFunc = e => e.Name;

        protected EntityController(IEntityAccessor<T> accessor)
        {
            Accessor = accessor;
        }

        protected virtual IEnumerable<T> Filter(T entity, List<T> list)
        {
            var filtered = new FilteredList<T>(list);
            filtered.Where(entity.Id, item => item.Id == entity.Id);
            filtered.Where(entity.Name, item => item.Name.ToLowerOrEmpty().Contains(entity.Name.ToLower()));
            return filtered.ToEnumerable();
        }

        [HttpGet]
        [Route("Get")]
        public virtual JsonResult Get(T entity)
        {
            var list = Filter(entity, Accessor.Get()).OrderBy(OrderByFunc);
            return Json(list.ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPut]
        [Route("Put")]
        public virtual ActionResult Put(T entity)
        {
            Accessor.Put(entity);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("Post")]
        public virtual ActionResult Post(T entity)
        {
            Accessor.Post(entity);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpDelete]
        [Route("Delete")]
        public virtual ActionResult Delete(T entity)
        {
            Accessor.Delete(entity);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}