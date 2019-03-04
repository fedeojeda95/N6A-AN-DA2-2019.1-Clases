using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Homeworks.DataAccess
{
    public class Repository<T>: IRepository<T> where T : class
    {
        protected DbContext Context { get; set; }

        public Repository(DbContext Context)
        {
            this.Context = Context;
        }
 
        public virtual IEnumerable<T> GetAll()
        {
            return Context.Set<T>();
        }
 
        public virtual IEnumerable<T> GetByCondition(Expression<Func<T, bool>> expression)
        {
            return Context.Set<T>().Where(expression);
        }
 
        public virtual T GetFirst(Expression<Func<T, bool>> expression)
        {
            return Context.Set<T>().First(expression);
        }
 
        public void Add(T entity)
        {
            Context.Set<T>().Add(entity);
        }
 
        public void Update(T entity)
        {
            Context.Set<T>().Update(entity);
        }
 
        public void Remove(T entity)
        {
            Context.Set<T>().Remove(entity);
        }
 
        public void Save()
        {
            Context.SaveChanges();
        }

        // Disposing

        private bool disposedValue = false;

        protected void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Context.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
  }
}