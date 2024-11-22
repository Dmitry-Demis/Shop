using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Shop.DAL.Services.DataAccess
{
    public class DataAccessService(IDataAccessStrategy dataAccessStrategy)
    {
        public void ConfigureRepositories(IServiceCollection services, IConfiguration configuration) => dataAccessStrategy.RegisterRepositories(services, configuration);
    }
}
