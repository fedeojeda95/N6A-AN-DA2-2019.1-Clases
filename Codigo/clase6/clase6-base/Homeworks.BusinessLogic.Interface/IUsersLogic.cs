using System;
using System.Collections.Generic;
using Homeworks.Domain;

namespace Homeworks.BusinessLogic.Interface
{
    public interface IUsersLogic : IDisposable
    {
        User Create(User user);

        User Get(Guid id);

        IEnumerable<User> GetUsers();
    }
}