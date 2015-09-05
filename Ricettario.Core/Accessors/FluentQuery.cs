using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using ServiceStack.OrmLite;

namespace Ricettario.Core.Accessors
{
    public interface IEasyData
    {
        T Single<T>(Expression<Func<T, bool>> predicate);
        int Update<T>(T entity);
        int Delete<T>(Expression<Func<T, bool>> predicate);
        List<T> Select<T>();
        long Insert<T>(T entity);
    }

    public class FluentQuery : IEasyData, IDisposable
    {
        private readonly IDbConnection _db;

        public FluentQuery(IDbConnection db)
        {
            _db = db;
        }

        public T Single<T>(Expression<Func<T, bool>> predicate)
        {
            return _db.Single(predicate);
        }

        public int Update<T>(T entity)
        {
            return _db.Update(entity);
        }

        public int Delete<T>(Expression<Func<T, bool>> predicate)
        {
            return _db.Delete(predicate);
        }

        public List<T> Select<T>()
        {
            return _db.Select<T>();
        }

        public long Insert<T>(T entity)
        {
            return _db.Insert(entity, selectIdentity: true);
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}