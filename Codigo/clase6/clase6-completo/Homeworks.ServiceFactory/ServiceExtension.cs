using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Homeworks.ServiceFactory
{
    public static class BLServiceCollectionServiceExtensions
    {
        public static IServiceCollection AddLogic<T>(this IServiceCollection service)
            where T : class
        {
            BusinessLogicFactory businessLogicFactory = new BusinessLogicFactory();
            Type typeToRegister = businessLogicFactory.GetImplementation<T>();
            return service.AddScoped(typeof(T), typeToRegister);
        }

        public static IServiceCollection AddRepository<T>(this IServiceCollection service)
            where T : class
        {
            RepositoryFactory repositoryFactory = new RepositoryFactory();
            Type typeToRegister = repositoryFactory.GetImplementation<T>();
            return service.AddScoped(typeof(T), typeToRegister);
        }

        public static IServiceCollection AddRepository(this IServiceCollection service, Type type)
        {
            RepositoryFactory repositoryFactory = new RepositoryFactory();
            Type typeToRegister = repositoryFactory.GetImplementation(type);
            return service.AddScoped(type, typeToRegister);
        }
    }
}