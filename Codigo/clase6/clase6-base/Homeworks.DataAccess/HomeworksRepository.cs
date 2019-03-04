using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

using Homeworks.Domain;

namespace Homeworks.DataAccess
{
  public class HomeworksRepository : Repository<Homework>
  {
    public HomeworksRepository(DbContext Context) : base(Context) { }

        public override IEnumerable<Homework> GetAll()
        {
            return Context.Set<Homework>().Include("Exercises");
        }
 
        public override IEnumerable<Homework> GetByCondition(Expression<Func<Homework, bool>> expression)
        {
            return Context.Set<Homework>().Include("Exercises").Where(expression);
        }
 
        public override Homework GetFirst(Expression<Func<Homework, bool>> expression)
        {
            return Context.Set<Homework>().Include("Exercises").First(expression);
        }
  }
}