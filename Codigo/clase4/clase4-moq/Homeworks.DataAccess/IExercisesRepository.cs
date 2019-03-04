using System;
using System.Collections.Generic;

using Homeworks.Domain;

namespace Homeworks.DataAccess
{
    public interface IExercisesRepository: IDisposable
    {
        Exercise Get(Guid id);

        IEnumerable<Exercise> GetAll();

        void Add(Exercise entity);

        void Remove(Exercise entity);

        void Update(Exercise entity);

        void Save();
    }
}