using DomainObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Storage
{
    public class MemoryRepository<T> : IDataRepository<T> where T : IEntity
    {
        Dictionary<Guid, T> db;

        public MemoryRepository()
        {
            this.db = new Dictionary<Guid, T>();
        }

        public void Add(T entity)
        {
            var newId = Guid.NewGuid();
            entity.Id = newId;
            this.db.Add(entity.Id, entity);
        }

        public void Delete(Guid id)
        {
            this.db.Remove(id);
        }

        public T GetById(Guid id)
        {
            return this.db[id];
        }

        public IEnumerable<T> GetWhere(Expression<Func<T, bool>> func)
        {
            var predicate = func.Compile();
            return this.db.Values.Where(predicate);
        }

        public void Update(T entity)
        {
            if (this.db.ContainsKey(entity.Id))
                this.db[entity.Id] = entity;
            else
                throw new KeyNotFoundException($"Entity {entity.Id} does not exist!");
        }
    }
}
