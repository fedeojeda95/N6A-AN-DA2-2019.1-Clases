using System;

namespace Homeworks.BusinessLogic
{
    public interface ISessionsLogic: IDisposable
    {
        Guid Login(string username, string password);
        bool IsValidToken(Guid token);
        bool HasLevel(Guid token, string role);
    }
}