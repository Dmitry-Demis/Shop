using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StoreCatalogDAL.StorageRegistration
{
    public interface IDataStorageTypeStrategy
    {
        void RegisterRepositories(IServiceCollection services, IConfiguration configuration);
    }
}
