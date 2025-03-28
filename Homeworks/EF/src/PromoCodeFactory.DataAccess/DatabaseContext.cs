using Microsoft.EntityFrameworkCore;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromoCodeFactory.DataAccess
{
    public class DatabaseContext: DbContext
    {
        public DbSet<PromoCode> PromoCodes { get; set; }
        public DbSet<Preference> Preferences { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<CustomerPreference> CustomerPreferences { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>(_ =>
            {
                _.HasOne(e => e.Role)
                 .WithMany()
                 .HasForeignKey(emp => emp.RoleId)
                 .IsRequired();
                _.Property(e => e.FirstName).HasMaxLength(100);
                _.Property(e => e.LastName).HasMaxLength(100);
                _.Property(e => e.Email).HasMaxLength(100);
                _.Property(e => e.AppliedPromocodesCount).HasDefaultValue(0);
            });


            modelBuilder.Entity<PromoCode>(_ =>
            {
                _.HasOne(pc => pc.PartnerManager)
                .WithMany()
                .HasForeignKey(pc => pc.PartnerManagerId);

                _.HasOne(pc => pc.Preference)
                 .WithMany()
                 .HasForeignKey(pc => pc.PreferenceId);

                _.HasOne(pc => pc.Customer)
                 .WithMany()
                 .HasForeignKey(pc => pc.CustomerId);

                _.Property(pc => pc.Code).HasMaxLength(20);
                _.Property(pc => pc.ServiceInfo).HasMaxLength(100);
                _.Property(pc => pc.PartnerName).HasMaxLength(100);
            });

            modelBuilder.Entity<Customer>(_ => { 
                _.HasMany(с => с.Promocodes)
                  .WithOne(p =>p.Customer)
                  .IsRequired();
                _.HasMany(c => c.CustomerPreferences)
                  .WithOne(p => p.Customer)
                  .IsRequired();

                _.Property(c => c.FirstName).HasMaxLength(100);
                _.Property(c => c.LastName).HasMaxLength(100);
                _.Property(c => c.Email).HasMaxLength(100);
            });

            modelBuilder.Entity<CustomerPreference>(_ =>
            {
                _.HasKey(cp => new { cp.CustomerId, cp.PreferenceId });

                _.HasOne(cp => cp.Preference)
                    .WithMany(p => p.CustomerPreferences)
                    .HasForeignKey(cp => cp.PreferenceId);

                _.HasOne(cp => cp.Customer)
                    .WithMany(c => c.CustomerPreferences)
                    .HasForeignKey(cp => cp.CustomerId);
            });

            modelBuilder.Entity<Role>().Property(e => e.Name).HasMaxLength(100);
            modelBuilder.Entity<Role>().Property(e => e.Description).HasMaxLength(200);
            modelBuilder.Entity<Preference>().Property(p => p.Name).HasMaxLength(100);
        }

    }
}
