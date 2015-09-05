using System;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using Ricettario.Core.Abstract;
using ServiceStack.OrmLite;

namespace Ricettario.Core.SubServices
{
    public class IngredientsSubService : SubServiceWithParentBase<Ingredient, Recipe>, ISubService
    {
        public IngredientsSubService(IDbConnection db, IHttpServiceBase service)
            : base(db, service, "ingredient")
        {
        }

        protected override void MapRequest(EntityUnifiedRequest request, Ingredient entity)
        {
            entity.Description = request.Description;
            entity.ProductId = request.Product;
        }

        protected override Recipe GetParent(EntityUnifiedRequest request)
        {
            return Db.Single<Recipe>(s => s.Id == request.ParentId);
        }

        protected override object GetData(EntityUnifiedRequest request)
        {
            var parent = GetParent(request);
            if (request.Action == "backjson")
            {
                var rows = parent.Ingredients.Select(r => new
                {
                    r.Id,
                    r.Description,
                    Product = r.ProductId,
                    Action = new object[] {},
                });
                return JsonConvert.SerializeObject(rows);
            }
            if (String.IsNullOrEmpty(request.Action) || request.Action == "load")
            {
                var products = Db.Select<Product>();

                var columns = new[]
                {
                    new GridColumn() {Name = "Id", Type = "id"},
                    new GridColumn() {Name = "Description", Type = "string"},
                    new GridColumn() {Name = "Product", Type = "lookup", Select2Cell = products.OrderBy(s => s.Name).Select(s => new object[] {s.Name, s.Id})},
                    new GridColumn() {Name = "", Type = "action"}
                };
                
                return new EntityUnifiedResponse()
                {
                    Type = request.Type,
                    Title = "Ingredients for " + parent.Name,
                    Columns = columns,
                    Url = request.Type + "/" + request.ParentId
                };

            }
            return null;
        }
    }
}