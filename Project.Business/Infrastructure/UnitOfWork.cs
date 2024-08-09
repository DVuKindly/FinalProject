using Project.Business.Infrastructure.Repository;
using Project.Data.Data;
using Project.Data.Entity;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Business.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ProjectDbContext _appDbContext;

        public UnitOfWork(ProjectDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

       

        private IBaseRepository<Role>? _roleRepository;
        public IBaseRepository<Role> RoleRepository
        {
            get
            {
                if (_roleRepository == null)
                {
                    _roleRepository = new RoleRepository(_appDbContext);
                }
                return _roleRepository;
            }
        }

        private IBaseRepository<User>? _UserRepository;
        public IBaseRepository<User> UserRepository
        {
            get
            {
                if (_UserRepository == null)
                {
                    _UserRepository = new UserRepository(_appDbContext);
                }
                return _UserRepository;
            }
        }

        private IBaseRepository<UserRole>? _UserRoleRepository;
        public IBaseRepository<UserRole> UserRoleRepository
        {
            get
            {
                if (_UserRoleRepository == null)
                {
                    _UserRoleRepository = new UserRoleRepository(_appDbContext);
                }
                return _UserRoleRepository;
            }
        }
        public IBaseRepository<AuditTrail> _AuditTrailRepository;
        public IBaseRepository<AuditTrail> AuditTrailRepository
        {
            get
            {
                if (_AuditTrailRepository == null)
                {
                    _AuditTrailRepository = new AuditTrailRepository(_appDbContext);
                }
                return _AuditTrailRepository;
            }
        }

        public IBaseRepository<Candidate> _CandidateRepository;
        public IBaseRepository<Candidate> CandidateRepository
        {
            get
            {
                if (_CandidateRepository == null)
                {
                    _CandidateRepository = new CandidateRepository(_appDbContext);
                }
                return _CandidateRepository;
            }
        }

        public IBaseRepository<Event> _EventRepository;
        public IBaseRepository<Event> EventRepository
        {
            get
            {
                if (_EventRepository == null)
                {
                    _EventRepository = new EventRepository(_appDbContext);
                }
                return _EventRepository;
            }
        }

        public IBaseRepository<RemarkHistory> _RemarkHistoryRepository;
        public IBaseRepository<RemarkHistory> RemarkHistoryRepository
        {
            get
            {
                if (_RemarkHistoryRepository == null)
                {
                    _RemarkHistoryRepository = new RemarkHistoryRepository(_appDbContext);
                }
                return _RemarkHistoryRepository;
            }
        }

        public IBaseRepository<SourcingHistory> _SourcingHistoryRepository;
        public IBaseRepository<SourcingHistory> SourcingHistoryRepository
        {
            get
            {
                if (_SourcingHistoryRepository == null)
                {
                    _SourcingHistoryRepository = new SourcingHistoryRepository(_appDbContext);
                }
                return _SourcingHistoryRepository;
            }
        }

        public IBaseRepository<UpdateHistory> _UpdateHistoryRepository;
        public IBaseRepository<UpdateHistory> UpdateHistoryRepository
        {
            get
            {
                if (_UpdateHistoryRepository == null)
                {
                    _UpdateHistoryRepository = new UpdateHistoryRepository(_appDbContext);
                }
                return _UpdateHistoryRepository;
            }
        }


        public int SaveChanges()
        {
            return _appDbContext.SaveChanges();
        }

        public IBaseRepository<T> GenericRepository<T>() where T : class
        {
            return new BaseRepository<T, ProjectDbContext>(_appDbContext);
        }


        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _appDbContext.SaveChangesAsync();
        }


        public async Task<int> SaveChangesAsync()
        {
            return await _appDbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            _appDbContext.Dispose();
        }
    }
}
