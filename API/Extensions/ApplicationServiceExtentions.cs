using API.Data;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
    public static class ApplicationServiceExtentions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<DataContext>(option => option.UseSqlServer(config.GetConnectionString("DatingApiConnectString")));
            services.AddCors();
            services.AddScoped<ITokenService, TokenService>();
            return services;
        }

    }
}
