using System;
using System.Collections.Generic;

using Homeworks.DataAccess;
using Homeworks.Domain;

namespace Homeworks.BusinessLogic
{
    public class ExerciseLogic : IDisposable
    {
        private ExercisesRepository exerciseRepository;

        public ExerciseLogic() {
            HomeworksContext context = ContextFactory.GetNewContext();
            exerciseRepository = new ExercisesRepository(context);
        }

        public void Create(Exercise exercise) {
            exerciseRepository.Add(exercise);
            exerciseRepository.Save();
        }

        public void Remove(Guid id) {
            Exercise exercise = exerciseRepository.Get(id);
            if (exercise == null) {
                throw new ArgumentException("Invalid guid");
            }
            exerciseRepository.Remove(exercise);
            exerciseRepository.Save();
        }

        public void Update(Guid id, Exercise exercise) {
            Exercise exerciseToUpdate = exerciseRepository.Get(id);

            if (exercise == null) {
                throw new ArgumentException("Invalid guid");
            }

            exerciseToUpdate.Problem = exercise.Problem;
            exerciseToUpdate.Score = exercise.Score;

            exerciseRepository.Update(exerciseToUpdate);
            exerciseRepository.Save();
        }

        public Exercise Get(Guid id) {
            return exerciseRepository.Get(id);
        }

        public IEnumerable<Exercise> GetExercises() {
            return exerciseRepository.GetAll();
        }

        public void Dispose()
        {
            exerciseRepository.Dispose();
        }
    }
}