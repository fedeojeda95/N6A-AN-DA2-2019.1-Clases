using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

using Homeworks.Domain;

namespace Homeworks.DataAccess
{
  public class SessionsRepository : Repository<Session>
  {
    public SessionsRepository(DbContext Context) : base(Context) { }

    public override IEnumerable<Session> GetAll()
    {
        return Context.Set<Session>().Include("User");
    }

    public override IEnumerable<Session> GetByCondition(Expression<Func<Session, bool>> expression)
    {
        return Context.Set<Session>().Include("User").Where(expression);
    }

    public override Session GetFirst(Expression<Func<Session, bool>> expression)
    {
        return Context.Set<Session>().Include("User").First(expression);
    }
  }
}