using Mediator;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Infrastructure.Helpers
{     /*Note : Ce helper est utilise par les behaviors pour savoir si le message en cours est une Command (ecriture) 
       * ou une Query (lecture), afin d'appliquer un traitement conditionnel.*/
    public static class TypeExtensions
    {
        public static bool IsCommand(this Type type)
        {
            if (type is null)
                return false;

            if (typeof(ICommand).IsAssignableFrom(type))
                return true;

            return type.GetInterfaces()
                .Any(i => i.IsGenericType &&
                           i.GetGenericTypeDefinition() == typeof(ICommand<>));
        }

        public static bool IsQuery(this Type type)
        {
            if (type is null)
                return false;

            return type.GetInterfaces()
                .Any(i => i.IsGenericType &&
                           i.GetGenericTypeDefinition() == typeof(IQuery<>));
        }
    }
}
