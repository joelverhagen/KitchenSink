using System;
using FluentAssertions;
using Knapcode.KitchenSink.Extensions;
using Knapcode.KitchenSink.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Knapcode.KitchenSink.Tests.Extensions
{
    [TestClass]
    public class RandomExtensionsTests
    {
        [TestMethod, TestCategory("Unit")]
        public void NextTimeSpan_WithMaximumLessThanMinimum_ThrowsException()
        {
            // ARRANGE, ACT
            Action action = () => RandomProvider.GetThreadRandom().NextTimeSpan(TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(1));

            // ASSERT
            action.ShouldThrow<ArgumentException>().And.ParamName.Should().Be("maximum");
        }

        [TestMethod, TestCategory("Unit")]
        public void NextTimeSpan_WithSameMaximumAsMinimum_Succeeds()
        {
            // ARRANGE
            TimeSpan expected = TimeSpan.FromMinutes(1);

            // ARRANGE, ACT
            var actual = RandomProvider.GetThreadRandom().NextTimeSpan(expected, expected);

            // ASSERT
            actual.Should().Be(expected);
        }

        [TestMethod, TestCategory("Unit")]
        public void NextTimeSpan_WithDifferentMaximumAndMinimum_Succeeds()
        {
            // ARRANGE
            TimeSpan minimum = TimeSpan.FromDays(0);
            TimeSpan maximum = TimeSpan.FromDays(1);

            // ARRANGE, ACT
            var actual = RandomProvider.GetThreadRandom().NextTimeSpan(minimum, maximum);

            // ASSERT
            actual.Should().BeGreaterOrEqualTo(minimum);
            actual.Should().BeLessOrEqualTo(maximum);
        }

        [TestMethod, TestCategory("Unit")]
        public void NextDateTime_WithMaximumLessThanMinimum_ThrowsException()
        {
            // ARRANGE, ACT
            Action action = () => RandomProvider.GetThreadRandom().NextDateTime(new DateTime(2000, 1, 1), new DateTime(1999, 1, 1));

            // ASSERT
            action.ShouldThrow<ArgumentException>().And.ParamName.Should().Be("maximum");
        }

        [TestMethod, TestCategory("Unit")]
        public void NextDateTime_WithSameMaximumAsMinimum_Succeeds()
        {
            // ARRANGE
            var expected = new DateTime(2000, 1, 1);

            // ARRANGE, ACT
            var actual = RandomProvider.GetThreadRandom().NextDateTime(expected, expected);

            // ASSERT
            actual.Should().Be(expected);
        }

        [TestMethod, TestCategory("Unit")]
        public void NextDateTime_WithDifferentMaximumAndMinimum_Succeeds()
        {
            // ARRANGE
            var minimum = new DateTime(1999, 1, 1);
            var maximum = new DateTime(2000, 1, 1);

            // ARRANGE, ACT
            var actual = RandomProvider.GetThreadRandom().NextDateTime(minimum, maximum);

            // ASSERT
            actual.Should().BeOnOrAfter(minimum);
            actual.Should().BeOnOrBefore(maximum);
        }

        [TestMethod, TestCategory("Unit")]
        public void NextDateTimeOffset_WithMaximumLessThanMinimum_ThrowsException()
        {
            // ARRANGE, ACT
            Action action = () => RandomProvider.GetThreadRandom().NextDateTimeOffset(new DateTimeOffset(new DateTime(2000, 1, 1)), new DateTimeOffset(new DateTime(1999, 1, 1)));

            // ASSERT
            action.ShouldThrow<ArgumentException>().And.ParamName.Should().Be("maximum");
        }

        [TestMethod, TestCategory("Unit")]
        public void NextDateTimeOffset_WithSameMaximumAsMinimum_Succeeds()
        {
            // ARRANGE
            var expected = new DateTimeOffset(new DateTime(2000, 1, 1));

            // ARRANGE, ACT
            var actual = RandomProvider.GetThreadRandom().NextDateTimeOffset(expected, expected);

            // ASSERT
            actual.Should().Be(expected);
        }

        [TestMethod, TestCategory("Unit")]
        public void NextDateTimeOffset_WithDifferentMaximumAndMinimum_Succeeds()
        {
            // ARRANGE
            var minimum = new DateTimeOffset(new DateTime(1999, 1, 1));
            var maximum = new DateTimeOffset(new DateTime(2000, 1, 1));

            // ARRANGE, ACT
            var actual = RandomProvider.GetThreadRandom().NextDateTimeOffset(minimum, maximum);

            // ASSERT
            actual.Should().BeOnOrAfter(minimum);
            actual.Should().BeOnOrBefore(maximum);
        }
    }
}
