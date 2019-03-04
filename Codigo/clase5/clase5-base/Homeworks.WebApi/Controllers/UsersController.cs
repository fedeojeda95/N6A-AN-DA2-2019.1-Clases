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
    public class UsersController : ControllerBase, IDisposable
    {
        private IUsersLogic usersLogic;

        public UsersController(IUsersLogic usersLogic = null)
        {
            if (usersLogic == null) {
                this.usersLogic = new UsersLogic();
            } else {
                this.usersLogic = usersLogic;
            }
        }

        [HttpGet]
        public IActionResult Get()
        {
            IEnumerable<User> users = usersLogic.GetUsers();
            IEnumerable<UserDTO> usersToReturn = users.Select(x => new UserDTO(x));
            return Ok(usersToReturn);
        }

        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            User user = usersLogic.Get(id);
            if (user == null) {
                return NotFound();
            }
            UserDTO userToReturn = new UserDTO(user);
            return Ok(userToReturn);
        }

        [HttpPost]
        public IActionResult Post([FromBody] UserDTO userDTO)
        {
            try {
                User userToCreate = userDTO.ToEntity();
                User createdUser = usersLogic.Create(userToCreate);
                UserDTO userToReturn = new UserDTO(createdUser);

                return CreatedAtRoute("Get", new { id = userToReturn.Id }, userToReturn);
            } catch(ArgumentException e) {
                return BadRequest(e.Message);
            }
        }

        public void Dispose()
        {
            usersLogic.Dispose();
        }
    }
}