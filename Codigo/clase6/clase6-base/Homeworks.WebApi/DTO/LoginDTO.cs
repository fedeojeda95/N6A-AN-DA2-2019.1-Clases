using System;
using System.Collections.Generic;
using System.Linq;
using Homeworks.Domain;

namespace Homeworks.WebApi.DTO
{
    public class LoginDTO
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        public LoginDTO() { }
    }
}