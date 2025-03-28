using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.DataAccess;
using PromoCodeFactory.DataAccess.Data;
using PromoCodeFactory.DataAccess.Repositories;
using System;
using System.Text.Json.Serialization;
using PromoCodeFactory.WebHost.Mapping;

namespace PromoCodeFactory.WebHost
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
             Configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                });            
            services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseSqlite(Configuration.GetConnectionString("ConnectionString"));
                options.LogTo(Console.WriteLine, LogLevel.Information);
                options.EnableSensitiveDataLogging();
            });

            InstallAutomapper(services);

            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IRepository<PromoCode>, EfRepository<PromoCode>>();
            services.AddScoped<IRepository<Preference>, EfRepository<Preference>>();
            services.AddScoped<IRepository<Role>, EfRepository<Role>>();
            services.AddScoped<IRepository<CustomerPreference>, EfRepository<CustomerPreference>>();

            

            services.AddOpenApiDocument(options =>
            {
                options.Title = "PromoCode Factory API Doc";
                options.Version = "1.0";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseOpenApi();
            app.UseSwaggerUi(x =>
            {
                x.DocExpansion = "list";
            });
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static IServiceCollection InstallAutomapper(IServiceCollection services)
        {
            services.AddSingleton<IMapper>(new Mapper(GetMapperConfiguration()));
            return services;
        }

        private static MapperConfiguration GetMapperConfiguration()
        { 

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new CustomerProfile());
                cfg.AddProfile(new PromocodeProfiler());
                cfg.AddProfile(new EmployeeProfile());
            });
            mapperConfig.AssertConfigurationIsValid();
            return mapperConfig;
        }

    }
}