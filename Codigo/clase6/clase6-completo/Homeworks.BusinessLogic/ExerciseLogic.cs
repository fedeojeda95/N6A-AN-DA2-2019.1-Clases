using System;
using System.Collections.Generic;

using Homeworks.Domain;
using Homeworks.BusinessLogic.Interface;
using Homeworks.DataAccess.Interface;

namespace Homeworks.BusinessLogic
{
  public class ExerciseLogic : IDisposable, IExerciseLogic
    {
        private IRepository<Exercise> exerciseRepository;

        public ExerciseLogic(IRepository<Exercise> exerciseRepository) {
            this.exerciseRepository = exerciseRepository;
        }

        public void Create(Exercise exercise) {
            exerciseRepository.Add(exercise);
            exerciseRepository.Save();
        }

        public void Remove(Guid id) {
            Exercise exercise = exerciseRepository.GetFirst(x => x.Id == id);
            if (exercise == null) {
                throw new ArgumentException("Invalid guid");
            }
            exerciseRepository.Remove(exercise);
            exerciseRepository.Save();
        }

        public void Update(Guid id, Exercise exercise) {
            Exercise exerciseToUpdate = exerciseRepository.GetFirst(x => x.Id == id);

            if (exercise == null) {
                throw new ArgumentException("Invalid guid");
            }

            exerciseToUpdate.Problem = exercise.Problem;
            exerciseToUpdate.Score = exercise.Score;

            exerciseRepository.Update(exerciseToUpdate);
            exerciseRepository.Save();
        }

        public Exercise Get(Guid id) {
            return exerciseRepository.GetFirst(x => x.Id == id);
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