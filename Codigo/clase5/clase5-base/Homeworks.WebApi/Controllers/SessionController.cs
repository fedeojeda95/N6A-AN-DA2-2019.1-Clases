using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

using Homeworks.Domain;
using Homeworks.WebApi.DTO;
using Homeworks.BusinessLogic;

namespace Homeworks.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class SessionsController : ControllerBase, IDisposable
    {
        private ISessionsLogic sessionsLogic;

        public SessionsController(ISessionsLogic sessionsLogic = null)
        {
            if (sessionsLogic == null) {
                this.sessionsLogic = new SessionsLogic();
            } else {
                this.sessionsLogic = sessionsLogic;
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO login) {
            try {
                Guid token = sessionsLogic.Login(login.UserName, login.Password);
                if (token == null) 
                {
                    return BadRequest("Invalid user/password");
                }
                return Ok(token);
            } catch(ArgumentException exception) {
                return BadRequest(exception.Message);
            }
        }

        public void Dispose()
        {
            sessionsLogic.Dispose();
        }
    }
}