﻿using API.Data;
using API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace API.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected AplicationContext _context;

        public Repository(AplicationContext context)
        {
            _context = context;
        }

        public T Create(T entity)
        {
            _context.Set<T>().Add(entity);
            return entity;
        }
        public async Task<IEnumerable<T>> GetAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }
        public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().SingleOrDefaultAsync(predicate);
        }

        public T Update(T entity)
        {
            var entityUpdated =_context.Set<T>().Update(entity);
            return entityUpdated.Entity;
        }

        public bool Delete(int id)
        {
            T? entity = _context.Set<T>().Find(id);
            if (entity == null) return false;
            var entityDeleted = _context.Set<T>().Remove(entity);
            return entityDeleted != null;
        }

    }
}
