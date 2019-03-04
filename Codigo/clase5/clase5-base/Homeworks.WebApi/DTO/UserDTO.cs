using System;
using System.Collections.Generic;
using System.Linq;
using Homeworks.Domain;

namespace Homeworks.WebApi.DTO
{
    public class UserDTO
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }

        public UserDTO() { }

        public UserDTO(User entity)
        {
            SetModel(entity);
        }

        public User ToEntity() => new User()
        {
            Id = this.Id,
            Name = this.Name,
            UserName = this.UserName,
            Password = this.Password,
            Role = this.Role,
        };

        protected UserDTO SetModel(User entity)
        {
            Id = entity.Id;
            Name = entity.Name;
            UserName = entity.UserName;
            Role = entity.Role;
            return this;
        }

    }
}