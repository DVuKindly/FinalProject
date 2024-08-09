using Project.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Business.Infrastructure
{
    public interface IUnitOfWork : IDisposable
    //IDisposable nó tự giải phóng bộ nhớ
    {
      
        IBaseRepository<User> UserRepository { get; }
        IBaseRepository<Role> RoleRepository { get; }
        IBaseRepository<UserRole> UserRoleRepository { get; }



        IBaseRepository<AuditTrail> AuditTrailRepository { get; }
        IBaseRepository<Candidate> CandidateRepository { get; }

        IBaseRepository<Event> EventRepository { get; }

        IBaseRepository<RemarkHistory> RemarkHistoryRepository { get; }

        IBaseRepository<SourcingHistory> SourcingHistoryRepository { get; }
        IBaseRepository<UpdateHistory> UpdateHistoryRepository { get; }





        IBaseRepository<T> GenericRepository<T>() where T : class;


        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);



        Task<int> SaveChangesAsync();
    }
}
