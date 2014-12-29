using System;

namespace Knapcode.KitchenSink.Extensions
{
    public static class RandomExtensions
    {
        public static TimeSpan NextTimeSpan(this Random random, TimeSpan minimum, TimeSpan maximum)
        {
            if (maximum < minimum)
            {
                throw new ArgumentException("The provided maximum TimeSpan is less than the minimum.", "maximum");
            }

            double ticks = random.NextDouble()*(maximum - minimum).Ticks;
            var roundedTicks = (long) Math.Round(ticks, 0);
            return minimum + TimeSpan.FromTicks(roundedTicks);
        }

        public static DateTime NextDateTime(this Random random, DateTime minimum, DateTime maximum)
        {
            return minimum + random.NextTimeSpan(TimeSpan.Zero, maximum - minimum);
        }

        public static DateTimeOffset NextDateTimeOffset(this Random random, DateTimeOffset minimum, DateTimeOffset maximum)
        {
            return minimum + random.NextTimeSpan(TimeSpan.Zero, maximum - minimum);
        }
    }
}