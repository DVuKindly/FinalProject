using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASP_PROJECT_OJT.Business.Service
{
    public interface IBaseService<T> where T : class
    {

      
        Task<int> AddAsync(T entity);
        Task<bool> UpdateAsync(T entity);

      
        bool Delete(Guid id);

        Task<bool> DeleteAsync(Guid id);

        Task<bool> DeleteAsync(T entity);

        Task<T?> GetByIdAsync(Guid id);

        Task<IEnumerable<T>> GetAllAsync();

      
    }
}
