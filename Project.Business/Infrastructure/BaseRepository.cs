using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Project.Business.Infrastructure
{
    public class BaseRepository<T, TC> : IBaseRepository<T>
       where T : class
       where TC : DbContext
    {


        protected readonly TC dataContext;
        protected readonly DbSet<T> dbset;


        public BaseRepository(TC dbContext)
        {
            this.dataContext = dbContext;
            dbset = dbContext.Set<T>();
        }


        public void Add(T entity)
        {
            dbset.Add(entity);
        }


        public void Add(IEnumerable<T> entities)
        {
            dbset.AddRange(entities);

        }


        public virtual void Delete(T entity, bool isHardDelete = false)
        {
            dbset.Remove(entity);
            dataContext.Entry(entity).State = EntityState.Deleted;
        }


        public virtual void Delete(Expression<Func<T, bool>> where, bool isHardDelete = false)
        {
            var entities = GetQuery(where).AsEnumerable();
            foreach (var entity in entities)
            {
                Delete(entity, isHardDelete);
            }
        }



        public void Delete(Guid id)
        {
            var entity = dbset.Find(id);
            if (entity != null)
            {
                dbset.Remove(entity);
            }
        }


        public virtual T GetById(Guid Id)
        {
            return dbset.Find(Id);
        }


        public IQueryable<T> GetQuery()
        {
            return dbset.AsQueryable();
        }


        public IQueryable<T> GetQuery(Expression<Func<T, bool>> where)
        {
            return GetQuery().Where(where);
        }


        public virtual void Update(T entity)
        {
            dataContext.Entry(entity).State = EntityState.Modified;
        }


        public void Update(List<T> entity)
        {
            entity.ForEach(a => Update(a));
        }


        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await dbset.FindAsync(id);
        }


        public void DeleteRange(IEnumerable<T> entities, bool isHardDelete = false)
        {
            dbset.RemoveRange(entities);
            foreach (var entity in entities)
            {
                dataContext.Entry(entity).State = EntityState.Deleted;
            }
            dataContext.SaveChanges();
        }
    }
}
