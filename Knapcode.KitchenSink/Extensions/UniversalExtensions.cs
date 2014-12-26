namespace Knapcode.KitchenSink.Extensions
{
    public static class UniversalExtensions
    {
        public delegate bool TryGetValue<in TInput, TOutput>(TInput input, out TOutput value);

        public  static TOutput GetTryValue<TInput, TOutput>(this TInput input, TryGetValue<TInput, TOutput> tryGetValue)
        {
            TOutput value;
            if (!tryGetValue(input, out value))
            {
                value = default(TOutput);
            }

            return value;
        }
    }
}
