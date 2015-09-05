using System.Data;
using System.Linq;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Ricettario.Core.Accessors
{
    public class DepartmentAccessor : EntityAccessor<Store>
    {
        public DepartmentAccessor(IDbConnectionFactory factory)
            : base(factory)
        {
        }

        public DepartmentAccessor(IDbConnection connection)
            : base(connection)
        {
        }

        public void Delete(int parentId, int entityId)
        {
            var location = new Location() { DepartmentId = entityId, StoreId = parentId };

            var parent = GetById(parentId);
            var deleted = parent.Departments.Single(d => d.Id == entityId);
            var replacements = parent.Departments.Where(d => d.Name.ToLower() == deleted.Name.ToLower() && d.Id != deleted.Id);
            var replacement = replacements.FirstOrDefault();
            var replacementId = replacement != null ? replacement.Id : 0;

            using (var db = GetConnection())
            {
                var products = db.Select<Product>().Where(r => r.WhereToBuy.Equals(location));
                foreach (var product in products)
                {
                    product.WhereToBuy.DepartmentId = replacementId;
                    db.Update(product);
                }
            }

            parent.Childs.Remove(deleted);
            Put(parent);
        }
    }
}