using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Principal;
using System.Threading.Tasks;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain;
namespace PromoCodeFactory.DataAccess.Repositories
{
    public class InMemoryRepository<T> : IRepository<T> where T : BaseEntity
    {
        protected List<T> Data { get; set; }  // используем List<T> вместо Enumerable<T> для удобства
        public InMemoryRepository(IEnumerable<T> data)
        {
            Data = data.ToList();
        }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            return Task.FromResult(Data.AsEnumerable());
        }

        public Task<T> GetByIdAsync(Guid id)
        {
            return Task.FromResult(Data.FirstOrDefault(x => x.Id == id));
        }

        public Task<T> CreateAsync(T entity)
        {   
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }
            Data.Add(entity);
            return Task.FromResult(entity);
        }
        public Task DeleteAsync(Guid id)
        {
            Data.Remove(Data.FirstOrDefault(x => x.Id == id));
            return Task.CompletedTask;
        }
        public Task UpdateAsync(T entity)
        {
            var index = Data.FindIndex(x => x.Id == entity.Id);
            if (index < 0)
            {
                throw new InvalidOperationException($"Элемент списка с кодом {entity.Id} не найден");
            }
            Data[index] = entity;
            return Task.CompletedTask;
        }

    }
}