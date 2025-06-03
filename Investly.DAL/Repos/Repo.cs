using Investly.DAL.Entities;
using Investly.DAL.Helper;
using Investly.DAL.Repos.IRepos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Investly.DAL.Repos
{
   public class Repo<T> : IRepo<T> where T : class
    {
        private readonly AppDbContext _db;
        private readonly DbSet<T> dbSet;
        public Repo(AppDbContext db)
        {
            _db = db;
           this.dbSet = _db.Set<T>();
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? properties = null)
        {
            IQueryable<T> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (properties != null)
            {
                foreach (var prop in properties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(prop);
                }
            }
            return query.ToList();
        }

        public void Remove(int id)
        {
            T entity = dbSet.Find(id);
            dbSet.Remove(entity);

        }

        public void RemoveRange(IEnumerable<T> entites)
        {
            dbSet.RemoveRange(entites);

        }
        public T GetById(int id)
        {
            T entity = dbSet.Find(id);
            return entity;
        }

        public void Insert(T entity)
        {
            dbSet.Add(entity);
        }

        public T FirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (includeProperties != null)
            {
                foreach (var prop in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(prop);
                }
            }
            return query.FirstOrDefault();
        }
        public void Update(T entity)
        {
            dbSet.Update(entity);
        }
        public void AddRange(IEnumerable<T> entites)
        {
            dbSet.AddRange(entites);
        }


        public IQueryable<T> FindAll(Expression<Func<T, bool>>? filter = null, string? properties = null)
        {
            IQueryable<T> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrWhiteSpace(properties))
            {
                foreach (var prop in properties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var trimmedProp = prop.Trim();
                    if (!string.IsNullOrEmpty(trimmedProp))
                    {
                        query = query.Include(trimmedProp);
                    }
                }
            }

            return query;
        }


    }
}
