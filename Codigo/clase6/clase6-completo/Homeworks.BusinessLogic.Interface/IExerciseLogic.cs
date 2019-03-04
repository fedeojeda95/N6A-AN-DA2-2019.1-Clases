using System;
using System.Collections.Generic;

using Homeworks.Domain;

namespace Homeworks.BusinessLogic.Interface
{
    public interface IExerciseLogic: IDisposable
    {
        void Create(Exercise exercise);

        void Remove(Guid id);

        void Update(Guid id, Exercise exercise);

        Exercise Get(Guid id);

        IEnumerable<Exercise> GetExercises();
    }
}