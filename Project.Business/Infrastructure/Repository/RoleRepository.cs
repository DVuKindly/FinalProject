
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


    public class RoleRepository : BaseRepository<Role, ProjectDbContext>
    {
        public RoleRepository(ProjectDbContext context) : base(context)
        {
        }

    }



}