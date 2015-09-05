using System.Data;
using System.Linq;
using Ricettario.Core.Abstract;
using ServiceStack.OrmLite;

namespace Ricettario.Core.SubServices
{
    public abstract class SubServiceWithParentBase<T, TP> where T : IIdentifiable, new() where TP : IParent<T>
    {
        protected readonly IHttpServiceBase _service;
        protected readonly string _type;
        protected IDbConnection Db { get; set; }

        protected SubServiceWithParentBase(IDbConnection db, IHttpServiceBase service, string type)
        {
            _service = service;
            _type = type;
            Db = db;
        }

        protected abstract void MapRequest(EntityUnifiedRequest request, T entity);

        protected abstract TP GetParent(EntityUnifiedRequest request);

        protected abstract object GetData(EntityUnifiedRequest request);

        public object Post(EntityUnifiedRequest request)
        {
            var parent = GetParent(request);
            if (request.Action == "remove")
            {
                Delete(request, parent);
            }
            else if (request.Action == "change")
            {
                var entity = parent.Childs.Single(d => d.Id == request.Id);

                MapRequest(request, entity);
            }
            else if (request.Action == "add")
            {
                var entity = AddChild(parent);
                MapRequest(request, entity);
            }
            Db.Update(parent);
            return new object();
        }

        protected virtual void Delete(EntityUnifiedRequest request, TP parent)
        {
            parent.Childs = parent.Childs.Where(d => d.Id != request.Id).ToList();
        }

        public object Get(EntityUnifiedRequest request)
        {
            if (request.Action == "add")
            {
                var parent = GetParent(request);
                AddChild(parent);
                Db.Update(parent);
                return _service.Redirect("/entity/" + _type + "/" + request.ParentId + "/load");
            }
            var response = GetData(request);
            return response;
        }

        private T AddChild(TP parent)
        {
            var nextId = parent.Childs.Max(d => d.Id) + 1;
            var child = New(nextId);
            parent.Childs.Add(child);
            return child;
        }
        
        protected virtual T New(int id)
        {
            return new T() {Id = id};
        }

        public bool CanHandle(string type)
        {
            return type.ToLower() == _type;
        }
    }
}