
using Microsoft.AspNetCore.Mvc;

using Project.Data.Data;

namespace EFCoreDemoApplication.Service
{
    public abstract class BaseService : Controller
    {
        protected readonly ProjectDbContext _context;
        public BaseService(ProjectDbContext context)
        {
            _context = context;
        }
    }
}
