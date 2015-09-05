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
    public class RecipeSubService : SubServiceBase<Recipe>, ISubService
    {
        public RecipeSubService(IDbConnection db, IHttpServiceBase service)
            : base(db, service, "recipe", "Recipes")
        {
        }

        protected override void MapRequest(EntityUnifiedRequest request, Recipe entity)
        {
            var accessor = new RecipeAccessor(Db);
            var newRecipe = accessor.CreateNewRecipe(new NewRecipe()
            {
                Name = request.Name,
                Reference = request.Reference,
                Description = request.Description,
                Tags = request.Tags,
                Ingredients = request.Ingredients
            });
            entity.Name = newRecipe.Name;
            entity.Reference = newRecipe.Reference;
            entity.Description = newRecipe.Description;
            if (entity.Id == 0)
            {
                entity.Tags = newRecipe.Tags;
                entity.Ingredients = newRecipe.Ingredients;
            }
        }

        protected override object GetData(EntityUnifiedRequest request)
        {
            if (request.Action == "backjson")
            {
                IEnumerable<Recipe> source = Db.Select<Recipe>();
                if (request.Field == "Tags")
                {
                    source = source.Where(r => r.Tags.Contains(request.Value));
                }
                var rows = source.OrderBy(p => p.Name).Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.Reference,
                    r.Description,
                    r.Tags,
                    Ingredients = "/entity/ingredient/" + r.Id + "/load",
                    Action = new object[] {},
                });
                return JsonConvert.SerializeObject(rows);
            }
            if (String.IsNullOrEmpty(request.Action) || request.Action == "filterByTag")
            {
                var columns = new[]
                {
                    new GridColumn() {Name = "Id", Type = "id"},
                    new GridColumn() {Name = "Name", Type = "string"},
                    new GridColumn() {Name = "Reference", Type = "string"},
                    new GridColumn() {Name = "Description", Type = "string"},
                    new GridColumn() {Name = "Tags", Type = "string"},
                    new GridColumn() {Name = "Ingredients", Type = "uri"},
                    new GridColumn() {Name = "", Type = "action"}
                };

                return CreateResponse(request, columns);
            }
            return null;
        }
    }
}