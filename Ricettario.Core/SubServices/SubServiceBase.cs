using System.Data;
using Ricettario.Core.Abstract;
using ServiceStack.OrmLite;

namespace Ricettario.Core.SubServices
{
    public abstract class SubServiceBase<T> where T : IIdentifiable, new()
    {
        protected readonly IHttpServiceBase _service;
        protected readonly string _type;
        protected readonly string _title;
        protected IDbConnection Db { get; set; }

        protected SubServiceBase(IDbConnection db, IHttpServiceBase service, string type, string title)
        {
            _service = service;
            _type = type;
            _title = title;
            Db = db;
        }

        public object Post(EntityUnifiedRequest request)
        {
            if (request.Action == "remove")
            {
                Delete(request);
            }
            else if (request.Action == "change")
            {
                var entity = Db.Single<T>(p => p.Id == request.Id);

                MapRequest(request, entity);

                Db.Update(entity);
            }
            else if (request.Action == "add")
            {
                var entity = New();

                MapRequest(request, entity);

                Db.Insert(entity);
            }
            return new object();
        }

        protected virtual void Delete(EntityUnifiedRequest request)
        {
            Db.Delete<T>(p => p.Id == request.Id);
        }

        public object Get(EntityUnifiedRequest request)
        {
            if (request.Action == "add")
            {
                Db.Insert(New());
                return _service.Redirect("/entity/" + _type);
            }
            var response = GetData(request);
            return response;
        }

        public bool CanHandle(string type)
        {
            return type.ToLower() == _type;
        }

        protected virtual T New()
        {
            return new T();
        }

        protected abstract void MapRequest(EntityUnifiedRequest request, T entity);

        protected abstract object GetData(EntityUnifiedRequest request);

        protected object CreateResponse(EntityUnifiedRequest request, GridColumn[] columns)
        {
            return new EntityUnifiedResponse()
            {
                Type = request.Type,
                Title = _title,
                Columns = columns,
                Field = request.Field,
                Value = request.Value
            };
        }
    }
}