using System;
using System.Collections.Generic;
using System.Data;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Ricettario.Core.Accessors
{
    public interface IEntityAccessor<T> where T : IIdentifiable
    {
        List<T> Get();
        T GetById(int id);
        int Put(T entity);
        long Post(T entity);
        int Delete(T entity);
    }

    public class EntityAccessor<T> : IEntityAccessor<T> where T : IIdentifiable
    {
        private readonly IDbConnection _connection;
        private readonly IDbConnectionFactory _factory;

        public EntityAccessor(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public EntityAccessor(IDbConnection connection)
        {
            _connection = connection;
        }

        protected IDbConnection GetConnection()
        {
            return _connection ?? _factory.OpenDbConnection();
        }

        public virtual List<T> Get()
        {
            using (var db = GetConnection())
            {
                return db.Select<T>();
            }
        }

        public virtual T GetBy(Func<T, bool> predicate)
        {
            using (var db = GetConnection())
            {
                return db.Single<T>(predicate);
            }
        }

        public virtual T GetById(int id)
        {
            using (var db = GetConnection())
            {
                return db.Single<T>(e => e.Id == id);
            }
        }

        public virtual int Put(T entity)
        {
            using (var db = GetConnection())
            {
                return db.Update(entity);
            }
        }

        public virtual long Post(T entity)
        {
            using (var db = GetConnection())
            {
                return db.Insert(entity);
            }
        }

        public virtual int Delete(T entity)
        {
            using (var db = GetConnection())
            {
                var id = entity.Id;
                return db.Delete<T>(arg => arg.Id == id);
            }
        }
    }
}