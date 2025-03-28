using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PromoCodeFactory.DataAccess;
using PromoCodeFactory.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PromoCodeFactory.Core.Domain.Administration;

namespace PromoCodeFactory.WebHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            var host = CreateHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                InitDatabase(context);
            }
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

        public static void InitDatabase(DatabaseContext context)
        {
            bool existsDb = context.Database.CanConnect();
            if (existsDb)
            {
                try
                {
                    var entityType = context.Model.FindEntityType(typeof(Employee));
                    var columnExists = entityType?.GetProperties()
                        .Any(p => p.GetColumnName() == "fakefield") ?? false;
                    if (!columnExists)
                        throw new Exception();
                }
                catch
                {
                    // если поле не существует - удаляем базу
                    context.Database.EnsureDeleted();
                }
            }
            // Всегда применяем миграции
            context.Database.Migrate();
            // Инициализация данных только если база пустая
            if (!context.Employees.Any())
            {
                FakeDataFactory.SeedData(context);
            }
        }
    }
}