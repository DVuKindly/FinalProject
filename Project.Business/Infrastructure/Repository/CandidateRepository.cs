using Project.Data.Data;
using Project.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Business.Infrastructure.Repository
{
    


    public class CandidateRepository : BaseRepository<Candidate, ProjectDbContext>
    {
        public CandidateRepository(ProjectDbContext context) : base(context)
        {
        }

    }

}
