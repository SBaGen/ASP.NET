using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain;

namespace PromoCodeFactory.DataAccess.Repositories
{
    public class EfRepository<T> : IRepository<T> where T : BaseEntity 
    {
        protected readonly DatabaseContext _context;
        private readonly DbSet<T> _entitySet;

        public EfRepository(DatabaseContext context)
        {
            _context = context;
            _entitySet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync() => await _entitySet.ToListAsync();
        public async Task<T> GetByIdAsync(Guid id) => await _entitySet.FindAsync(id);

        public async Task<T> CreateAsync(T entity)
        {
            await _entitySet.AddAsync(entity);
            return entity;
        }
        public async Task UpdateAsync(T entity)
        {
            _entitySet.Update(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }
        public async Task DeleteAsync(T entity)
        {
            _entitySet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
