using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PromoCodeFactory.Core.Abstractions.Repositories
{
    /// <summary>
    /// Репозиторий для работы с заказчиками
    /// </summary>
        
    public interface ICustomerRepository:IRepository<Customer>
    {
        /// <summary>   
        /// Список всех заказчиков с деталями
        /// </summary>
        Task<IEnumerable<Customer>> GetAllWithDetailsAsync();
        /// <summary>
        /// Получить заказчика по Id с деталями
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Customer> GetWithDetailsByIdAsync(Guid id);
    }

}
