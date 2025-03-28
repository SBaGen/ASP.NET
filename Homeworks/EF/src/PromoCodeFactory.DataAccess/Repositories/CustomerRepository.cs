using Microsoft.EntityFrameworkCore;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromoCodeFactory.DataAccess.Repositories
{
    public class CustomerRepository : EfRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(DatabaseContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Customer>> GetAllWithDetailsAsync()
        {
            return await _context.Customers
                .Include(c => c.CustomerPreferences)
                    .ThenInclude(cp => cp.Preference)
                .Include(c => c.Promocodes)
                .ToListAsync();
        }
        public async Task<Customer> GetWithDetailsByIdAsync(Guid id)
        {
            return await _context.Customers
                .Include(c => c.CustomerPreferences)
                    .ThenInclude(cp => cp.Preference)
                .Include(c => c.Promocodes)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
