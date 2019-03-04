using System;

namespace Homeworks.Domain
{
    public class Session
    {
        public Guid Id {get; set;}
        public Guid Token {get; set;}
        public User User {get; set;}

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