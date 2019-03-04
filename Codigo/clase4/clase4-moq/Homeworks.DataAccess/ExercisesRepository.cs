using System;
using System.Collections.Generic;
using System.Linq;
using Homeworks.Domain;
using Microsoft.EntityFrameworkCore;

namespace Homeworks.DataAccess
{
    public class ExercisesRepository: IDisposable, IExercisesRepository
    {
        // 1 - Creacion 
        protected DbContext Context {get; set;}

        public ExercisesRepository(DbContext context)
        {
            Context = context;
        }

        // 2- Acceso y manipulacion de datos en la DB

        public Exercise Get(Guid id)
        {
            return Context.Set<Exercise>().First(x => x.Id == id);
        }

        public IEnumerable<Exercise> GetAll()
        {
            return Context.Set<Exercise>().ToList();
        }

        public void Add(Exercise entity) {
            Context.Set<Exercise>().Add(entity);
        }

        public void Remove(Exercise entity) {
            Context.Set<Exercise>().Remove(entity);
        }

        public void Update(Exercise entity) {
            Context.Entry(entity).State = EntityState.Modified;
        }

        public void Save() {
            Context.SaveChanges();
        }

        // 3 - Disposing

        #region IDisposable Support

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
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

        #endregion
    }
}