using System;

using Homeworks.Domain;
using Homeworks.BusinessLogic.Interface;
using Homeworks.DataAccess.Interface;

namespace Homeworks.BusinessLogic
{
  public class SessionsLogic : ISessionsLogic
  {
    private IRepository<Session> sessionRepository;
    private IRepository<User> userRepository;

    public SessionsLogic(IRepository<Session> sessionRepository, IRepository<User> userRepository) {
        this.sessionRepository = sessionRepository;
        this.userRepository = userRepository;
    }

    public Guid Login(string username, string password)
    {
        Guid sessionToken = Guid.NewGuid();
        User user = userRepository.GetFirst(u => u.UserName == username && u.Password == password);
        if (user == null) {
            throw new ArgumentException("Username/Password not valid");
        }

        Session session = new Session() { Token = sessionToken, User = user };
        sessionRepository.Add(session);
        sessionRepository.Save();

        return sessionToken;
    }

    public bool IsValidToken(Guid token)
    {
      Session sessionForToken = sessionRepository.GetFirst(s => s.Token == token);
      return sessionForToken != null;
    }

    public bool HasLevel(Guid token, string role)
    {
        // Not really "clean code" like
        Session sessionForToken = sessionRepository.GetFirst(s => s.Token == token);
        User sessionUser = sessionForToken.User;

        if (sessionUser == null) {
          return false;
        }

        if (sessionUser.Role != role) {
          return false;
        }

        return true;
    }

    public void Dispose()
    {
        sessionRepository.Dispose();
        userRepository.Dispose();
    }

  }
}