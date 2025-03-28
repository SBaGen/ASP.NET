using PromoCodeFactory.Core.Domain.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromoCodeFactory.Core.Abstractions.Repositories
{
    public interface IEmployeeRepository: IRepository<Employee>
    {
        /// <summary>
        /// Получить сотрудника по Id с ролью
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Employee> GetByIdWithRoleAsync(Guid id);
    }
}
