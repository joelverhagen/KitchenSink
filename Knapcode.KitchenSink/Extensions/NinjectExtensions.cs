using System;
using System.Linq;
using Ninject.Activation;
using Ninject.Infrastructure.Language;
using Ninject.Syntax;

namespace Knapcode.KitchenSink.Extensions
{
    public static class NinjectExtensions
    {
        public static IBindingInNamedWithOrOnSyntax<T> WhenInjectedIntoDescendentOf<T>(this IBindingWhenSyntax<T> when, Type ancestor)
        {
            return when.When(r => IsTypeRequestedByAncestor(r, ancestor));
        }

        private static bool IsTypeRequestedByAncestor(IRequest r, Type ancestor)
        {
            while (r != null && !IsTypeInRequest(r, ancestor))
            {
                r = r.ParentRequest;
            }

            return r != null;
        }

        private static bool IsTypeInRequest(IRequest r, Type parent)
        {
            if (parent.IsGenericTypeDefinition)
            {
                if (parent.IsInterface)
                {
                    return r.Target != null &&
                           r.Target.Member != null &&
                           r.Target.Member.ReflectedType != null &&
                           r.Target.Member.ReflectedType.GetInterfaces().Any(i =>
                               i.IsGenericType &&
                               i.GetGenericTypeDefinition() == parent);
                }

                return
                    r.Target != null &&
                    r.Target.Member.ReflectedType.GetAllBaseTypes().Any(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == parent);
            }

            return r.Target != null && parent.IsAssignableFrom(r.Target.Member.ReflectedType);
        }
    }
}