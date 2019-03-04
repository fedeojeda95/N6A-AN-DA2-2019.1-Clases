using System;
using System.Collections.Generic;
using System.Linq;
using Homeworks.Domain;

namespace Homeworks.WebApi.DTO
{
    public class HomeworkDTO
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public List<ExerciseDTO> Exercises {get; set;}

        public HomeworkDTO()
        {
            Exercises = new List<ExerciseDTO>();
        }

        public HomeworkDTO(Homework entity)
        {
            SetModel(entity);
        }

        public Homework ToEntity() => new Homework()
        {
            Id = this.Id,
            Description = this.Description,
            DueDate = this.DueDate,
            Exercises = this.Exercises.ConvertAll(m => m.ToEntity()),
        };

        protected HomeworkDTO SetModel(Homework entity)
        {
            Id = entity.Id;
            Description = entity.Description;
            DueDate = entity.DueDate;
            Exercises = entity.Exercises.ConvertAll(m => new ExerciseDTO(m));
            return this;
        }

    }
}