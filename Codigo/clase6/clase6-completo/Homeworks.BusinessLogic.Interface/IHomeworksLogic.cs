using System;
using System.Collections.Generic;

using Homeworks.Domain;

namespace Homeworks.BusinessLogic.Interface
{
    public interface IHomeworksLogic: IDisposable
    {
        Homework Create(Homework homework);

        Exercise AddExercise(Guid id, Exercise exercise);

        void Remove(Guid id);

        Homework Update(Guid id, Homework homework);

        Homework Get(Guid id);

        IEnumerable<Homework> GetHomeworks();

    }
}