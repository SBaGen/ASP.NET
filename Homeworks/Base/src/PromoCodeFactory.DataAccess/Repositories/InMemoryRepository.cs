﻿using System;
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
        private List<T> _data; //Внутренний список для хранения данных
        protected IEnumerable<T> Data => _data.AsEnumerable(); //обеспечиваем инкапсуляцию 

        public InMemoryRepository(IEnumerable<T> data)
        {
            _data = data.ToList();
        }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            return Task.FromResult(Data);
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
            _data.Add(entity);
            return Task.FromResult(entity);
        }
        public Task DeleteAsync(Guid id)
        {
            _data.Remove(_data.FirstOrDefault(x => x.Id == id));
            return Task.CompletedTask;
        }
        public Task UpdateAsync(T entity)
        {
            var index = _data.FindIndex(x => x.Id == entity.Id);
            if (index < 0)
            {
                throw new InvalidOperationException($"Элемент списка с кодом {entity.Id} не найден");
            }
            _data[index] = entity;
            return Task.CompletedTask;
        }

    }
}