using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.Core.Abstractions.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;

namespace PromoCodeFactory.DataAccess.Repositories
{
    public class EmployeeRepository : EfRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(DatabaseContext context) : base(context)
        {
        }
        public async Task<Employee> GetByIdWithRoleAsync(Guid id)
        {
            return await _context.Employees
                .Include(e => e.Role) 
                .FirstOrDefaultAsync(e => e.Id == id);
        }

    }
}
