using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Ricettario;
using Ricettario.Core.Accessors;

namespace Ricettario.Controllers.Abstract
{
    public abstract class EntityWithParentController<TParent, T> : BaseController
        where TParent : IIdentifiable, IParent<T>, new()
        where T : IIdentifiable, new()
    {
        protected readonly IEntityAccessor<TParent> Accessor;
        protected Func<T, string> OrderByFunc = e => e.Name;

        protected EntityWithParentController(IEntityAccessor<TParent> accessor)
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

        protected virtual IEnumerable<T> OrderBy(IEnumerable<T> array)
        {
            return array.OrderBy(OrderByFunc);
        }

        [HttpGet]
        [Route("Get")]
        public virtual JsonResult Get(int parentId, T entity)
        {
            var parent = Accessor.GetById(parentId);

            var list = OrderBy(Filter(entity, (parent.Childs ?? new List<T>())));
            return Json(list.ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPut]
        [Route("Put")]
        public virtual ActionResult Put(int parentId, T entity)
        {
            var parent = Accessor.GetById(parentId);

            var index = parent.Childs.FindIndex(c => c.Id == entity.Id);
            parent.Childs.RemoveAt(index);
            parent.Childs.Insert(index, entity);
            parent.Childs = parent.Childs.OrderBy(c => c.Name).ToList();

            Accessor.Put(parent);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("Post")]
        public virtual ActionResult Post(int parentId, T entity)
        {
            var parent = Accessor.GetById(parentId);

            entity.Id = parent.Childs.Count == 0 ? 0 : parent.Childs.Max(d => d.Id) + 1;
            parent.Childs.Add(entity);
            parent.Childs = parent.Childs.OrderBy(c => c.Name).ToList();

            Accessor.Put(parent);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpDelete]
        [Route("Delete")]
        public virtual ActionResult Delete(int parentId, T entity)
        {
            var parent = Accessor.GetById(parentId);

            var index = parent.Childs.FindIndex(c => c.Id == entity.Id);
            parent.Childs.RemoveAt(index);

            Accessor.Put(parent);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}