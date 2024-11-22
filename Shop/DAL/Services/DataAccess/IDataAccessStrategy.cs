using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shop.DAL.Services.DataAccess
{
    public interface IDataAccessStrategy
    {
        void RegisterRepositories(IServiceCollection services, IConfiguration configuration);
    }
}
