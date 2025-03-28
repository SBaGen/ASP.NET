using System;
using System.Collections.Generic;
using System.Linq;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;

namespace PromoCodeFactory.DataAccess.Data
{
    public static class FakeDataFactory
    {
        public static IEnumerable<Employee> Employees => new List<Employee>()
        {
            new Employee()
            {
                Id = Guid.Parse("451533d5-d8d5-4a11-9c7b-eb9f14e1a32f"),
                Email = "owner@somemail.ru",
                FirstName = "Иван",
                LastName = "Сергеев",
                Role = Roles.FirstOrDefault(x => x.Name == "Admin"),
                AppliedPromocodesCount = 5
            },
            new Employee()
            {
                Id = Guid.Parse("f766e2bf-340a-46ea-bff3-f1700b435895"),
                Email = "andreev@somemail.ru",
                FirstName = "Петр",
                LastName = "Андреев",
                Role = Roles.FirstOrDefault(x => x.Name == "PartnerManager"),
                AppliedPromocodesCount = 10
            },
        };

        public static IEnumerable<Role> Roles => new List<Role>()
        {
            new Role()
            {
                Id = Guid.Parse("53729686-a368-4eeb-8bfa-cc69b6050d02"),
                Name = "Admin",
                Description = "Администратор",
            },
            new Role()
            {
                Id = Guid.Parse("b0ae7aac-5493-45cd-ad16-87426a5e7665"),
                Name = "PartnerManager",
                Description = "Партнерский менеджер"
            }
        };

        public static IEnumerable<Preference> Preferences => new List<Preference>()
        {
            new Preference()
            {
                Id = Guid.Parse("ef7f299f-92d7-459f-896e-078ed53ef99c"),
                Name = "Театр",
            },
            new Preference()
            {
                Id = Guid.Parse("c4bda62e-fc74-4256-a956-4760b3858cbd"),
                Name = "Семья",
            },
            new Preference()
            {
                Id = Guid.Parse("76324c47-68d2-472d-abb8-33cfa8cc0c84"),
                Name = "Дети",
            }
        };
        public static IEnumerable<Customer> Customers
        {
            get
            {
                var customerId = Guid.Parse("a6c8c6b1-4349-45b0-ab31-244740aaf0f0");
                var customers = new List<Customer>()
                {
                    new Customer()
                    {
                        Id = customerId,
                        Email = "ivan_sergeev@mail.ru",
                        FirstName = "Иван",
                        LastName = "Петров",
                        CustomerPreferences =  new List<CustomerPreference>
                        { 
                            new CustomerPreference()
                            {
                                Id = Guid.NewGuid(),
                                CustomerId = customerId,
                                PreferenceId = Preferences.FirstOrDefault(x => x.Name == "Театр").Id
                            }
                        }
                        //TODO: Добавить предзаполненный список предпочтений
                    }
                };
                return customers;
            }
        }
        public static IEnumerable<PromoCode> PromoCodes => new List<PromoCode>()
        {
            new PromoCode()
            {
                Id = Guid.Parse("CC199AB8-7412-4729-A1D4-577A6AC8452D"),
                BeginDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1),
                Code = "123456",
                ServiceInfo = "Скидка 10%",
                PartnerName = "Театр",
                PreferenceId = Preferences.FirstOrDefault(x => x.Name == "Театр").Id,
                CustomerId = Customers.FirstOrDefault().Id
            },
        };
        public static IEnumerable<CustomerPreference> CustomerPreferences => new List<CustomerPreference>()
        {
            new CustomerPreference()
            {
                Id = Guid.NewGuid(),
                CustomerId = Customers.FirstOrDefault().Id,
                PreferenceId = Preferences.FirstOrDefault(x => x.Name == "Театр").Id
            },
            new CustomerPreference()
            {
                Id = Guid.NewGuid(),
                CustomerId = Customers.FirstOrDefault().Id,
                PreferenceId = Preferences.FirstOrDefault(x => x.Name == "Дети").Id
            },
        };


        public static void SeedData(DatabaseContext context)
        {
            if (!context.Employees.Any())
            {
                context.Employees.AddRange(Employees);
                context.SaveChanges();
            }
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(Roles);
                context.SaveChanges();
            }
            if (!context.Preferences.Any())
            {
                context.Preferences.AddRange(Preferences);
                context.SaveChanges();
            }
            if (!context.Customers.Any())
            {
                context.Customers.AddRange(Customers);
                context.SaveChanges();
            }
            if (!context.PromoCodes.Any())
            {
                context.PromoCodes.AddRange(PromoCodes);
                context.SaveChanges();
            }
            if (!context.CustomerPreferences.Any())
            {
                context.CustomerPreferences.AddRange(CustomerPreferences);
                context.SaveChanges();
            }
        }


    }
}