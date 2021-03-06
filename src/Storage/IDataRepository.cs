﻿using DomainObjects;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Storage
{
    public interface IDataRepository<T> where T : IEntity
    {
        T GetById(Guid id);
        IEnumerable<T> GetWhere(Expression<Func<T, bool>> func);
        void Add(T entity);
        T Update(T entity);
        void Delete(Guid id);
    }
}
