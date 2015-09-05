using System;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using Ricettario.Core.Abstract;
using ServiceStack.OrmLite;

namespace Ricettario.Core.SubServices
{
    public class StoreSubService : SubServiceBase<Store>, ISubService
    {
        public StoreSubService(IDbConnection db, IHttpServiceBase service)
            : base(db, service, "store", "Stores")
        {
        }

        protected override void MapRequest(EntityUnifiedRequest request, Store entity)
        {
            entity.Name = request.Name;
        }

        protected override object GetData(EntityUnifiedRequest request)
        {
            if (request.Action == "backjson")
            {
                var rows = Db.Select<Store>().OrderBy(p => p.Name).Select(r => new
                {
                    r.Id,
                    r.Name,
                    Departments = "/entity/department/" + r.Id + "/load",
                    Action = new object[] {},
                });
                return JsonConvert.SerializeObject(rows);
            }
            if (String.IsNullOrEmpty(request.Action))
            {
                var columns = new[]
                {
                    new GridColumn() {Name = "Id", Type = "id"},
                    new GridColumn() {Name = "Name", Type = "string"},
                    new GridColumn() {Name = "Departments", Type = "uri"},
                    new GridColumn() {Name = "", Type = "action"}
                };

                return CreateResponse(request, columns);
            }
            return null;
        }
    }
}