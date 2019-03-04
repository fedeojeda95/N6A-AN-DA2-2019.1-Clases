using System;
using System.Collections.Generic;
using Homeworks.Domain;

namespace Homeworks.WebApi.DTO
{
    public class ExerciseDTO
    {
        public Guid Id { get; set; }
        public string Problem { get; set; }
        public int Score { get; set; }

        public ExerciseDTO() { }

        public ExerciseDTO(Exercise entity)
        {
            SetModel(entity);
        }

        public Exercise ToEntity() => new Exercise()
        {
            Id = this.Id,
            Problem = this.Problem,
            Score = this.Score,
        };

        protected ExerciseDTO SetModel(Exercise entity)
        {
            Id = entity.Id;
            Problem = entity.Problem;
            Score = entity.Score;
            return this;
        }
    }
}