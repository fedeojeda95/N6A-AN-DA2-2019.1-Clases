using System;
using System.Collections.Generic;

using Homeworks.Domain;

namespace Homeworks.DataAccess
{
    public interface IHomeworksRepository: IDisposable
    {
        Homework Get(Guid id);

        IEnumerable<Homework> GetAll();

        void Add(Homework entity);

        void Remove(Homework entity);

        void Update(Homework entity);

        void Save();
    }
}