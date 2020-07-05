using DomainObjects;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Storage
{
    public interface IDataRepository<T> where T : IEntity
    {
        T Get(Guid id);
        IEnumerable<T> GetWhere(Expression<Func<T, bool>> func);
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
