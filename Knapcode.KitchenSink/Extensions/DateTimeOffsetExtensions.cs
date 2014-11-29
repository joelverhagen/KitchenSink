using System;

namespace Knapcode.KitchenSink.Extensions
{
    public static class DateTimeOffsetExtensions
    {
        private const string FixedWidthLongFormat = "D19";

        public static string GetDescendingOrderString(this DateTimeOffset dateTimeOffset)
        {
            return (DateTimeOffset.MaxValue.Ticks - dateTimeOffset.Ticks).ToString(FixedWidthLongFormat);
        }

        public static string GetAscendingOrderString(this DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.Ticks.ToString(FixedWidthLongFormat);
        }
    }
}