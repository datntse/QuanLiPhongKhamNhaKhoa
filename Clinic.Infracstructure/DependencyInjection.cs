﻿using Clinic.Infracstructure.Repositories;
using Clinic.Infracstructure.Services;
using Clinic.Infracstruture.Data;
using Clinic.Infracstruture.Repositories;
using Clinic.Infracstruture.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.ConstrainedExecution;


namespace Clinic.Infracstruture
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
            #region entity
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();

            services.AddScoped<IClinicDentalsRepository, ClinicDentalsRepository>();
            services.AddScoped<IClinicDetailsService, ClinicDetailsService>();

            services.AddScoped<IDentistInfoRepository,DentistInfoRepository>();
            services.AddScoped<IDentistInfoService,DentistInfoService>();

            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IRoleService, RoleSerivce>();

            services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            services.AddScoped<IAppointmentService, AppointmentService>();

            #endregion
            return services;
        }
    }
}

