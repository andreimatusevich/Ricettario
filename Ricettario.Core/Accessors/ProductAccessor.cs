using System.Collections.Generic;
using System.Data;
using System.Linq;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Ricettario.Core.Accessors
{
    public class ProductAccessor : EntityAccessor<Product>
    {
        public ProductAccessor(IDbConnectionFactory factory)
            : base(factory)
        {
        }

        public ProductAccessor(IDbConnection connection)
            : base(connection)
        {
        }

        public override int Delete(Product entity)
        {
            var id = entity.Id;
            using (var db = GetConnection())
            {
                var deleted = db.Single<Product>(p => p.Id == id);
                var replacements = db.Select<Product>(p => p.Name.ToLower() == deleted.Name.ToLower() && p.Id != deleted.Id);
                var replacement = replacements.FirstOrDefault();
                var replacementId = replacement != null ? replacement.Id : 0;
                var recipes = db.Select<Recipe>().Where(r => r.Ingredients.Any(i => i.ProductId == deleted.Id));
                foreach (var recipe in recipes)
                {
                    var ingredients = recipe.Ingredients.Where(i => i.ProductId == deleted.Id);
                    foreach (var ingredient in ingredients)
                    {
                        ingredient.ProductId = replacementId;
                    }
                    db.Update(recipe);
                }
                return db.Delete<Product>(p => p.Id == id);
            }
        }
    }
}