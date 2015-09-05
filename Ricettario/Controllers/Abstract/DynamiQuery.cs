using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Ricettario.Controllers.Abstract
{
    public class DynamiQuery<T>
    {
        private IQueryable<T> _queryableData;

        public DynamiQuery(IQueryable<T> queryableData)
        {
            _queryableData = queryableData;
        }

        public void Where<TValue>(string property, TValue value)
        {
            var pe = Expression.Parameter(typeof(T), "item");
            var left = Expression.Property(pe, property);
            var right = Expression.Constant(value);
            var expression = Expression.Equal(left, right);

            var whereCallExpression = Expression.Call(
                typeof(Queryable),
                "Where",
                new[] { _queryableData.ElementType },
                _queryableData.Expression,
                Expression.Lambda<Func<T, bool>>(expression, new[] { pe }));

            _queryableData = _queryableData.Provider.CreateQuery<T>(whereCallExpression);
        }

        public List<T> ToList()
        {
            return _queryableData.ToList();
        }
    }

    //protected override List<Product> Filter(ProductSearch entity, List<Product> list)
    //{
    //    var query = new DynamiQuery<Product>(list.AsQueryable());
    //    if (entity.Id != default(int))
    //    {
    //        query.Where("Id", entity.Id);
    //    }
    //    return query.ToList();
    //}
}