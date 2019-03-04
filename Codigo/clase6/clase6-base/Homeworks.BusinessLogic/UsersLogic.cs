using System;
using System.Collections.Generic;

using Homeworks.DataAccess;
using Homeworks.Domain;
using Homeworks.BusinessLogic.Interface;
using Homeworks.DataAccess.Interface;

namespace Homeworks.BusinessLogic
{
    public class UsersLogic : IUsersLogic
    {
        private IRepository<User> repository;

        public UsersLogic(IRepository<User> repository) {
            this.repository = repository;
        }

        public UsersLogic() {
            HomeworksContext context = ContextFactory.GetNewContext();
            repository = new Repository<User>(context);
        }

        public User Create(User user) 
        {
            ValidateUser(user);
            repository.Add(user);
            repository.Save();
            return user;
        }

        public User Get(Guid id) {
            return repository.GetFirst(x => x.Id == id);
        }

        public IEnumerable<User> GetUsers() 
        {
            return repository.GetAll();
        }

        public void Dispose()
        {
            repository.Dispose();
        }

        private void ValidateUser(User user) 
        {
            // No es correcto del todo. Estas validaciones podrían estar en otro lugar
            if (user == null || !user.IsValid()) 
            {
                throw new ArgumentException("User not valid");
            }
        }
    }
}