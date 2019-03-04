using System;

namespace Homeworks.Domain
{
    public class Session
    {
        public Guid Id {get; set;}
        public Guid token {get; set;}
        public User user {get; set;}

        public Session()
        {
            Id = Guid.NewGuid();
        }

        public bool IsValid()
        {
            return true;
        }
    }
}