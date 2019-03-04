using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Homeworks.BusinessLogic.Interface;
using Homeworks.DataAccess.Interface;
using Homeworks.DataAccess;

namespace Homeworks.ServiceFactory
{
    public class RepositoryFactory
    {
        private string assemblyPath;

        public RepositoryFactory()
        {
            assemblyPath = @"/Users/federicoojeda/Desktop/Homeworks.DataAccess.dll";
        }

        public Type GetImplementation<T>() where T : class
        {
            Type implementationType = GetInstanceOfInterface<T>();
            return implementationType;
        }

        public Type GetImplementation(Type typeOfInterface)
        {
            Type implementationType = GetInstanceOfInterface(typeOfInterface);
            return implementationType;
        }

        private Type GetInstanceOfInterface<Interface>()
        {
            try
            {
                Assembly assembly = Assembly.LoadFile(assemblyPath);
                IEnumerable<Type> implementations = GetTypesInAssembly<Interface>(assembly);
                if (implementations.Count() <= 0)
                {
                    throw new NullReferenceException(assemblyPath + " don't contains Types that extend from " + nameof(Interface));
                }

                return implementations.First();
            }
            catch (Exception e)
            {
                throw new Exception("Can't load assembly " + assemblyPath, e);
            }
        }

        private Type GetInstanceOfInterface(Type interfaceToRegister)
        {
            try
            {
                Assembly assembly = Assembly.LoadFile(assemblyPath);
                IEnumerable<Type> implementations = GetTypesInAssembly(interfaceToRegister, assembly);
                if (implementations.Count() <= 0)
                {
                    throw new NullReferenceException(assemblyPath + " don't contains Types that extend from " + nameof(interfaceToRegister));
                }

                return implementations.First();
            }
            catch (Exception e)
            {
                throw new Exception("Can't load assembly " + assemblyPath, e);
            }
        }

        private static IEnumerable<Type> GetTypesInAssembly<Interface>(Assembly assembly)
        {
            List<Type> types = new List<Type>();
            foreach (var type in assembly.GetTypes())
            {
                var interfaceType = typeof(Interface);
                if (typeof(Interface).IsAssignableFrom(type))
                {
                    types.Add(type);
                }
            }
            return types;
        }

        private static IEnumerable<Type> GetTypesInAssembly(Type interfaceToRegister, Assembly assembly)
        {
            List<Type> types = new List<Type>();
            foreach (var type in assembly.GetTypes())
            {
                if (IsAssignableToGenericType(type, interfaceToRegister))
                {
                    types.Add(type);
                }
            }
            return types;
        }

        private static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            Type baseType = givenType.BaseType;
            Type[] interfaceTypes = givenType.GetInterfaces();

            // Checkeamos que haya una interfaz del tipo que sea generica y que sea top level (herede directamente de Object).
            // De esta manera nos evitamos que las subclases tambien cumplan la condición.
            foreach (Type it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType && baseType == typeof(Object))
                {
                    return true;
                }
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            {
                return true;
            }

            return false;
        }
    }
}