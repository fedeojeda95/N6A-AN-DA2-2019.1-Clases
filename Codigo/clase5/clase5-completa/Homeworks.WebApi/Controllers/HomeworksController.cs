using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using Homeworks.BusinessLogic;
using Homeworks.Domain;
using Homeworks.WebApi.DTO;

namespace Homeworks.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeworksController: ControllerBase, IDisposable
    {
        private IHomeworksLogic homeworksLogic;

        public HomeworksController(IHomeworksLogic homeworksLogic = null) {
            if (homeworksLogic == null) {
                this.homeworksLogic = new HomeworksLogic();
            } else {
                this.homeworksLogic = homeworksLogic;
            }
        }

        // GET api/homeworks
        [HttpGet]
        public ActionResult Get()
        {
            IEnumerable<Homework> homeworks = homeworksLogic.GetHomeworks();
            IEnumerable<HomeworkDTO> homeworksToReturn = homeworks.Select(x => new HomeworkDTO(x));
            return Ok(homeworksToReturn);
        }

        [HttpGet("{id}", Name = "Get")]
        public IActionResult Get(Guid id)
        {
            Homework homework = homeworksLogic.Get(id);
            if (homework == null) {
                return NotFound();
            }

            HomeworkDTO homeworkToReturn = new HomeworkDTO(homework);
            return Ok(homeworkToReturn);
        }

        [HttpPost]
        public IActionResult Post([FromBody] HomeworkDTO homeworkDTO)
        {
            try {
                Homework homework = homeworkDTO.ToEntity();
                Homework createdHomework = homeworksLogic.Create(homework);

                HomeworkDTO homeworkToReturn = new HomeworkDTO(createdHomework);
                return CreatedAtRoute("Get", new { id = homeworkToReturn.Id }, homeworkToReturn);
            } catch(ArgumentException e) {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("{id}/Exercises", Name = "AddExercise")]
        public IActionResult PostExercise(Guid id, [FromBody] ExerciseDTO exerciseDTO)
        {
            Exercise exercise = exerciseDTO.ToEntity();
            Exercise createdExercise = homeworksLogic.AddExercise(id, exercise);
            if (createdExercise == null) {
                return BadRequest();
            }

            ExerciseDTO exerciseToReturn = new ExerciseDTO(createdExercise);
            return CreatedAtRoute("GetExercise", new { id = exerciseToReturn.Id }, exerciseToReturn);
        }

        [HttpPut("{id}")]
        public IActionResult Put(Guid id, [FromBody] HomeworkDTO homeworkDTO)
        {
            try {
                Homework homework = homeworkDTO.ToEntity();
                Homework updatedHomework = homeworksLogic.Update(id, homework);

                HomeworkDTO homeworkToReturn = new HomeworkDTO(updatedHomework);
                return CreatedAtRoute("Get", new { id = homework.Id }, updatedHomework);
            } catch(ArgumentException e) {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            homeworksLogic.Remove(id);
            return NoContent();
        }

        public void Dispose()
        {
            homeworksLogic.Dispose();
        }
  }
}
