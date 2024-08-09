
using Microsoft.EntityFrameworkCore;
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
   


    public class UserRoleRepository : BaseRepository<UserRole, ProjectDbContext>
    {
        public UserRoleRepository(ProjectDbContext context) : base(context)
        {
        }

    }

}
