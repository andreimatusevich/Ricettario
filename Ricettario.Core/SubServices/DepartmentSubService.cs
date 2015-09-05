using System;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using Ricettario.Core.Abstract;
using Ricettario.Core.Accessors;
using ServiceStack.OrmLite;

namespace Ricettario.Core.SubServices
{
    public class DepartmentSubService : SubServiceWithParentBase<Department, Store>, ISubService
    {
        public DepartmentSubService(IDbConnection db, IHttpServiceBase service)
            : base(db, service, "department")
        {
        }

        protected override void MapRequest(EntityUnifiedRequest request, Department entity)
        {
            entity.Name = request.Name;
            entity.OrderHelper = request.OrderHelper;
        }

        protected override Store GetParent(EntityUnifiedRequest request)
        {
            return Db.Single<Store>(s => s.Id == request.ParentId);
        }

        protected override void Delete(EntityUnifiedRequest request, Store parent)
        {
            var accessor = new DepartmentAccessor(Db);
            accessor.Delete(parent.Id, request.Id);
            parent.Childs.RemoveAll(c => c.Id == request.Id);
        }

        protected override object GetData(EntityUnifiedRequest request)
        {
            var parent = GetParent(request);
            if (request.Action == "backjson")
            {
                var rows = parent.Departments.OrderBy(p => p.Name).Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.OrderHelper,
                    Action = new object[] { },
                });
                return JsonConvert.SerializeObject(rows);
            }
            if (String.IsNullOrEmpty(request.Action) || request.Action == "load")
            {
                var columns = new[]
                {
                    new GridColumn() {Name = "Id", Type = "id"},
                    new GridColumn() {Name = "Name", Type = "string"},
                    new GridColumn() {Name = "OrderHelper", Type = "string"},
                    new GridColumn() {Name = "", Type = "action"}
                };

                return new EntityUnifiedResponse()
                {
                    Type = request.Type,
                    Title = "Departments for " + parent.Name,
                    Columns = columns,
                    Url = request.Type + "/" + request.ParentId
                };

            }
            return null;
        }
    }
}