using System;

namespace Knapcode.KitchenSink.Support
{
    public static class Guard
    {
        public static void ArgumentNotNull<T>(T argument, string paramName) where T : class
        {
            if (argument == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}