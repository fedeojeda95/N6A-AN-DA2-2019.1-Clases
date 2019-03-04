using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using Homeworks.Domain;
using Homeworks.BusinessLogic;

namespace Homeworks.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class ExercisesController : ControllerBase, IDisposable
    {
        private IExerciseLogic exerciseLogic;

        public ExercisesController(IExerciseLogic exerciseLogic = null) {
            if (exerciseLogic == null) {
                this.exerciseLogic = new ExerciseLogic();
            } else {
                this.exerciseLogic = exerciseLogic;
            }
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(exerciseLogic.GetExercises());
        }

        [HttpGet("{id}", Name = "GetExercise")]
        public IActionResult Get(Guid id)
        {
            Exercise exercise = exerciseLogic.Get(id);
            if (exercise == null) {
                return NotFound();
            }
            return Ok(exercise);
        }

        public void Dispose()
        {
            exerciseLogic.Dispose();
        }

    }
}