using DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext dbContext;
        private DbSet<T> dbSet;
        public Repository(ApplicationDbContext dbContext)
        {

            this.dbContext = dbContext;
            dbSet = dbContext.Set<T>();

        }
        public IEnumerable<T> Get(Expression<Func<T, object>>[]? Include = null, Expression<Func<T, bool>>? expressio = null, bool tracked = true)

        {
            IQueryable<T> query = dbSet;
            if (expressio != null)
            {
                query = query.Where(expressio);
            }
            if (Include != null)
            {
                foreach (var prop in Include)
                {

                    query = query.Include(prop);
                }
            }
            if (!tracked)
            {
                query = query.AsNoTracking();
            }
            return query.ToList();
        }
        public T? GetOne(Expression<Func<T, object>>[]? includeProps = null, Expression<Func<T, bool>>? expression = null, bool tracked = true)
        {
            return Get(includeProps, expression, tracked).FirstOrDefault();
        }

        public void Create(T Entity)
        {
            dbSet.Add(Entity);


        }

        public void Edit(T Entity)
        {
            dbSet.Update(Entity);


        }

        public void Delete(T Entity)
        {
            dbSet.Remove(Entity);

        }

        public void commit()
        {
            dbContext.SaveChanges();
        }




    }
}
