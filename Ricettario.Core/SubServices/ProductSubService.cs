using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using Ricettario.Core.Abstract;
using Ricettario.Core.Accessors;
using ServiceStack.OrmLite;

namespace Ricettario.Core.SubServices
{
    public class ProductSubService : SubServiceBase<Product>, ISubService
    {
        public ProductSubService(IDbConnection db, IHttpServiceBase service)
            : base(db, service, "product", "Products")
        {
        }

        protected override void MapRequest(EntityUnifiedRequest request, Product entity)
        {
            var storeId = request.Store;
            var departmentId = Extension.FromDepartmentId(storeId, request.Department);

            var store = SelectStoreDepartments().Single(s => s.StoreId == storeId && s.DepartmentId == departmentId);

            entity.Name = request.Name;
            entity.WhereToBuy.StoreId = store.StoreId;
            entity.WhereToBuy.DepartmentId = store.DepartmentId;
            entity.Buy = request.Buy;

            if (!String.IsNullOrWhiteSpace(request.Synonyms))
            {
                entity.Synonyms = String.Join(",", request.Synonyms.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        protected override void Delete(EntityUnifiedRequest request)
        {
            var accessor = new ProductAccessor(Db);
            accessor.Delete(new Product() { Id = request.Id});
        }

        protected override object GetData(EntityUnifiedRequest request)
        {
            if (request.Action == "backjson")
            {
                var storeDepartments = SelectStoreDepartments().ToList();
                var products = Db.Select<Product>().OrderBy(p => p.Name);

                var join = from jp in products
                    join js in storeDepartments on jp.WhereToBuy equals new Location() { StoreId = js.StoreId, DepartmentId = js.DepartmentId }
                    select new { product = jp, store = js };

                var rows = @join.Select(r => new
                {
                    r.product.Id,
                    r.product.Name,
                    Synonyms = r.product.Synonyms ?? "",
                    Store = r.store.StoreId,
                    Department = Extension.ToDepartmentId(r.store.StoreId, r.store.DepartmentId),
                    r.product.Buy
                });

                return JsonConvert.SerializeObject(rows);
            }
            if (String.IsNullOrEmpty(request.Action))
            {
                var stores = Db.Select<Store>();
                var ds = stores.Select(
                        s => new Select2CellWithGroupsModel()
                        {
                            name = s.Name, 
                            values = s.Departments.OrderBy(d => d.Name).Select(d => new Object[] { d.Name ?? "", Extension.ToDepartmentId(s.Id, d.Id) })
                        });

                var columns = new[]
                {
                    new GridColumn() {Name = "Id", Type = "id"},
                    new GridColumn() {Name = "Name", Type = "string"},
                    new GridColumn() {Name = "Synonyms", Type = "string"},
                    new GridColumn() {Name = "Store", Type = "lookup", Select2Cell = stores.OrderBy(s => s.Name).Select(s => new object[] {s.Name, s.Id})},
                    new GridColumn()
                    {
                        Name = "Department",
                        Type = "lookup-group",
                        Select2CellWithGroups = ds
                    },
                    new GridColumn() {Name = "Buy", Type = "boolean"},
                    new GridColumn() {Name = "", Type = "action"}
                };
                
                return CreateResponse(request, columns);
            }
            return null;
        }

        protected IEnumerable<StoreDepartment> SelectStoreDepartments()
        {
            var stores =
                Db.Select<Store>()
                    .SelectMany(
                        s =>
                            s.Departments.Select(
                                d =>
                                    new StoreDepartment
                                    {
                                        StoreId = s.Id,
                                        Store = s.Name,
                                        DepartmentId = d.Id,
                                        Department = d.Name
                                    }));
            return stores;
        }
    }
}