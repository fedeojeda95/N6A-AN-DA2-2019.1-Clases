using System;

using Homeworks.Domain;
using Homeworks.DataAccess;

namespace Homeworks.BusinessLogic
{
  public class SessionsLogic : ISessionsLogic
  {
    private IRepository<Session> sessionRepository;
    private IRepository<User> userRepository;

    public SessionsLogic(SessionsRepository sessionRepository, IRepository<User> userRepository) {
        this.sessionRepository = sessionRepository;
        this.userRepository = userRepository;
    }

    public SessionsLogic() {
        HomeworksContext context = ContextFactory.GetNewContext();
        userRepository = new Repository<User>(context);
        sessionRepository = new SessionsRepository(context);
    }

    public Guid Login(string username, string password)
    {
        Guid sessionToken = Guid.NewGuid();
        User user = userRepository.GetFirst(u => u.UserName == username && u.Password == password);
        if (user == null) {
            throw new ArgumentException("Username/Password not valid");
        }

        Session session = new Session() { token = sessionToken, user = user };
        sessionRepository.Add(session);
        sessionRepository.Save();

        return sessionToken;
    }

    public void Dispose()
    {
        sessionRepository.Dispose();
        userRepository.Dispose();
    }

  }
}