using Project.Data.Data;
using Project.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Business.Infrastructure.Repository
{
 
    public class EventRepository : BaseRepository<Event, ProjectDbContext>
    {
        public EventRepository(ProjectDbContext context) : base(context)
        {
        }

    }


}
