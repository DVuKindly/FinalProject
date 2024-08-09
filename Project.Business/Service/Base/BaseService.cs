
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project.Business.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ASP_PROJECT_OJT.Business.Service
{
    public class BaseService<T> : IBaseService<T> where T : class
    {
        protected readonly ILogger<BaseService<T>> _logger;
        protected readonly IUnitOfWork _unitOfWork;

        public BaseService(IUnitOfWork unitOfWork, ILogger<BaseService<T>> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public virtual int Add(T entity)
        {
            if (entity == null)
            {
                _logger.LogError("Entity is null.");
                throw new ArgumentNullException(nameof(entity));
            }

            _unitOfWork.GenericRepository<T>().Add(entity);

            var result =  _unitOfWork.SaveChanges();

            if (result > 0)
            {
                _logger.LogInformation("Entity added successfully.");
            }
            else
            {
                _logger.LogError("Entity not added.");
            }

            return result;
        }

        public virtual async Task<int> AddAsync(T entity)
        {
            if (entity == null)
            {
                _logger.LogError("Entity is null.");
                throw new ArgumentNullException(nameof(entity));
            }

            _unitOfWork.GenericRepository<T>().Add(entity);
            var result = await _unitOfWork.SaveChangesAsync();

            if (result > 0)
            {
                _logger.LogInformation("Entity added successfully.");
            }
            else
            {
                _logger.LogError("Entity not added.");
            }

            return result;
        }

        public virtual async Task<bool> UpdateAsync(T entity)
        {
            if (entity == null)
            {
                _logger.LogError("Entity is null.");
                throw new ArgumentNullException(nameof(entity));
            }

            _unitOfWork.GenericRepository<T>().Update(entity);
            var result = await _unitOfWork.SaveChangesAsync() > 0;

            if (result)
            {
                _logger.LogInformation("Entity updated successfully.");
            }
            else
            {
                _logger.LogError("Entity not updated.");
            }

            return result;
        }

      
        public virtual bool Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                _logger.LogError("ID is empty.");
                throw new ArgumentNullException(nameof(id));
            }

            _unitOfWork.GenericRepository<T>().Delete(id);
            var result = _unitOfWork.SaveChanges() > 0;

            if (result)
            {
                _logger.LogInformation("Entity deleted successfully.");
            }
            else
            {
                _logger.LogError("Entity not deleted.");
            }

            return result;
        }

        public virtual async Task<bool> DeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                _logger.LogError("ID is empty.");
                throw new ArgumentNullException(nameof(id));
            }

            _unitOfWork.GenericRepository<T>().Delete(id);
            var result = await _unitOfWork.SaveChangesAsync() > 0;

            if (result)
            {
                _logger.LogInformation("Entity deleted successfully.");
            }
            else
            {
                _logger.LogError("Entity not deleted.");
            }

            return result;
        }

        public virtual async Task<bool> DeleteAsync(T entity)
        {
            if (entity == null)
            {
                _logger.LogError("Entity is null.");
                throw new ArgumentNullException(nameof(entity));
            }

            _unitOfWork.GenericRepository<T>().Delete(entity);
            var result = await _unitOfWork.SaveChangesAsync() > 0;

            if (result)
            {
                _logger.LogInformation("Entity deleted successfully.");
            }
            else
            {
                _logger.LogError("Entity not deleted.");
            }

            return result;
        }

        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            return await _unitOfWork.GenericRepository<T>().GetByIdAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _unitOfWork.GenericRepository<T>().GetQuery().ToListAsync();
        }

   
    }
}
