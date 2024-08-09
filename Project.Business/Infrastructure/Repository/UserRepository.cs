
using Project.Business.Infrastructure;
using Project.Data.Data;
using Project.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Business.Infrastructure.Repository
{
  

    public class UserRepository : BaseRepository<User, ProjectDbContext>
    {
        public UserRepository(ProjectDbContext context) : base(context)
        {
        }

    }

}
