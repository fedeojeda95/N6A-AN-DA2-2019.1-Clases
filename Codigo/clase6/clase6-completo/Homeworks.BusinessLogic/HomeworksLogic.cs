using System;
using System.Collections.Generic;

using Homeworks.Domain;
using Homeworks.BusinessLogic.Interface;
using Homeworks.DataAccess.Interface;

namespace Homeworks.BusinessLogic
{
    public class HomeworksLogic: IDisposable, IHomeworksLogic
    {
        private IRepository<Homework> homeworksRepository;

        public HomeworksLogic(IRepository<Homework> homeworksRepository)
        {
            this.homeworksRepository = homeworksRepository;
        }

        public Homework Create(Homework homework) 
        {
            homeworksRepository.Add(homework);
            homeworksRepository.Save();
            return homework;
        }

        public Exercise AddExercise(Guid id, Exercise exercise)
        {
            Homework homework = homeworksRepository.GetFirst(x => x.Id == id);
            if (homework == null) {
                throw new ArgumentException("Invalid guid");
            }
            homework.Exercises.Add(exercise);
            homeworksRepository.Update(homework);
            homeworksRepository.Save();
            return exercise;
        }

        public void Remove(Guid id) 
        {
            Homework homework = homeworksRepository.GetFirst(x => x.Id == id);
            if (homework == null) {
                throw new ArgumentException("Invalid guid");
            }
            homeworksRepository.Remove(homework);
            homeworksRepository.Save();
        }

        public Homework Update(Guid id, Homework homework)
        {
            Homework homeworkToUpdate = homeworksRepository.GetFirst(x => x.Id == id);
            if (homeworkToUpdate == null) {
                throw new ArgumentException("Invalid guid");
            }
            homeworkToUpdate.Description = homework.Description;
            homeworkToUpdate.DueDate = homework.DueDate;
            homeworksRepository.Update(homeworkToUpdate);
            homeworksRepository.Save();
            return homeworkToUpdate;
        }

        public Homework Get(Guid id) 
        {
            return homeworksRepository.GetFirst(x => x.Id == id);
        }

        public IEnumerable<Homework> GetHomeworks() 
        {
            return homeworksRepository.GetAll();
        }

        public void Dispose()
        {
            homeworksRepository.Dispose();
        }
  }
}