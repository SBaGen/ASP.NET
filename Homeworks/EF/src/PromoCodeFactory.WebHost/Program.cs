using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PromoCodeFactory.DataAccess;
using PromoCodeFactory.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;

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
            if (context.Database.CanConnect())
            {
                try
                {
                    // удал€ем если не было миграций после первого запуска
                    if (!hasFirstModified(context))
                        context.Database.EnsureDeleted();
                }
                catch
                {
                    // .. удаление на каждый запуск уже не должно происходить
                    // если что-то пошло не так - удал€ем базу
                    context.Database.EnsureDeleted();
                }
            }
            // ¬сегда примен€ем миграции
            context.Database.Migrate();
            // »нициализаци€ данных только если не было миграций после первого запуска
            if (!hasFirstModified(context))
            {
                FakeDataFactory.SeedData(context);
            }
        }

        public static bool hasFirstModified(DatabaseContext context)
        {
            // ѕровер€ем наличие таблицы отслеживани€
            var trackingTableExists = isValidExecSql(context, @"SELECT 1 FROM sqlite_master WHERE type='table' AND name='__SchemaTracking'");

            if (!trackingTableExists)
            {
                // ѕервоначальное создание базы
                isValidExecSql(context, @"
                        CREATE TABLE IF NOT EXISTS __SchemaTracking (
                            Id INTEGER PRIMARY KEY,
                            Initialized INTEGER NOT NULL DEFAULT 0,
                            FirstChangeDetected INTEGER NOT NULL DEFAULT 0
                        );
            
                        INSERT OR IGNORE INTO __SchemaTracking (Id, Initialized) 
                        VALUES (1, 1);", true);//noquery
                return false;
            }

            // ѕолучаем состо€ние отслеживани€
            var firstChangeDetected = isValidExecSql(context, "SELECT FirstChangeDetected FROM __SchemaTracking WHERE Id = 1");
            if (firstChangeDetected)
                return true;
            // ѕровер€ем наличие миграций
            var pendingMigrations = context.Database.GetPendingMigrations().Any();
            if (pendingMigrations)
            {
                // ќбновл€ем флаг, если есть миграции
                isValidExecSql(context, "UPDATE __SchemaTracking SET FirstChangeDetected = 1 WHERE Id = 1", true);//noquery
                return true;
            }
            return false;
        }

        public static bool isValidExecSql(DatabaseContext context, string sql, bool noquery = false)
        {
            var connection = context.Database.GetDbConnection();
            try
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                if (noquery)
                {
                    command.ExecuteNonQuery();
                    return true;
                }
                var result = command.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                {
                    return false;
                }
                return Convert.ToInt32(result) == 1;
            }

            finally
            {
                connection.Close();
            }
        }
    }
}