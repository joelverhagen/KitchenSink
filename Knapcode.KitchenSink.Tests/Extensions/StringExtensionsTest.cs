using System;
using FluentAssertions;
using FluentAssertions.Specialized;
using Knapcode.KitchenSink.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Knapcode.KitchenSink.Tests.Extensions
{
    /// <summary>
    /// Unit tests for <see cref="StringExtensions" />.
    /// </summary>
    [TestClass]
    public class StringExtensionsTest
    {
        [TestMethod, TestCategory("Unit")]
        public void SplitAtIndices_WithNullString_ThrowsArgumentNullException()
        {
            // ARRANGE
            const string input = null;

            // ACT
            Action action = () => input.SplitAtIndices(new[] {0}, StringSplitOptions.None);

            // ASSERT
            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("s");
        }

        [TestMethod, TestCategory("Unit")]
        public void SplitAtIndices_WithNullIndices_ThrowsArgumentNullException()
        {
            // ARRANGE
            const string input = "FooBarBaz";

            // ACT
            Action action = () => input.SplitAtIndices(null, StringSplitOptions.None);

            // ASSERT
            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("indices");
        }

        [TestMethod, TestCategory("Unit")]
        public void SplitAtIndices_WithOutOfOrderIndices_ThrowsArgumentException()
        {
            // ARRANGE
            const string input = "FooBarBaz";

            // ACT
            Action action = () => input.SplitAtIndices(new[] {1, 0}, StringSplitOptions.None);

            // ASSERT
            ExceptionAssertions<ArgumentException> expectation = action.ShouldThrow<ArgumentException>();
            expectation.And.ParamName.Should().Be("indices");
            expectation.And.Message.Should().Contain("ascending order");
        }

        [TestMethod, TestCategory("Unit")]
        public void SplitAtIndices_WithIndexIsTooLarge_ThrowsArgumentOutOfRangeException()
        {
            // ARRANGE
            const string input = "FooBarBaz";

            // ACT
            Action action = () => input.SplitAtIndices(new[] {2*input.Length}, StringSplitOptions.None);

            // ASSERT
            ExceptionAssertions<ArgumentOutOfRangeException> expectation = action.ShouldThrow<ArgumentOutOfRangeException>();
            expectation.And.ParamName.Should().Be("indices");
            expectation.And.Message.Should().Contain("length");
        }

        [TestMethod, TestCategory("Unit")]
        public void SplitAtIndices_WithIndexIsNegative_ThrowsArgumentOutOfRangeException()
        {
            // ARRANGE
            const string input = "FooBarBaz";

            // ACT
            Action action = () => input.SplitAtIndices(new[] {-1}, StringSplitOptions.None);

            // ASSERT
            ExceptionAssertions<ArgumentOutOfRangeException> expectation = action.ShouldThrow<ArgumentOutOfRangeException>();
            expectation.And.ParamName.Should().Be("indices");
            expectation.And.Message.Should().Contain("length");
        }

        [TestMethod, TestCategory("Unit")]
        public void SplitAtIndices_Always_CoversEntireString()
        {
            // ARRANGE
            const string input = "FooBarBaz";
            var indices = new[] {3, 4, 5, 6};

            // ACT
            string[] pieces = input.SplitAtIndices(indices, StringSplitOptions.None);

            // ASSERT
            string.Join(string.Empty, pieces).Should().Be(input);
        }

        [TestMethod, TestCategory("Unit")]
        public void SplitAtIndices_WithAllowingEmptyPieces_WorksAsExpected()
        {
            // ARRANGE
            const string input = "FooBarBaz";
            var indices = new[] {0, 0, 3, 6, 6, 9, 9};
            var expected = new[] {string.Empty, string.Empty, "Foo", "Bar", string.Empty, "Baz", string.Empty, string.Empty};

            // ACT
            string[] actual = input.SplitAtIndices(indices, StringSplitOptions.None);

            // ASSERT
            actual.ShouldBeEquivalentTo(expected);
        }

        [TestMethod, TestCategory("Unit")]
        public void SplitAtIndices_WithRemovingEmptyPieces_WorksAsExpected()
        {
            // ARRANGE
            const string input = "FooBarBaz";
            var indices = new[] {0, 0, 3, 6, 6, 9, 9};
            var expected = new[] {"Foo", "Bar", "Baz"};

            // ACT
            string[] actual = input.SplitAtIndices(indices, StringSplitOptions.RemoveEmptyEntries);

            // ASSERT
            actual.ShouldBeEquivalentTo(expected);
        }

        [TestMethod, TestCategory("Unit")]
        public void SplitAtIndices_WithInnerIndex_WorksAsExpected()
        {
            // ARRANGE
            const string input = "FooBarBaz";
            var indices = new[] {3};
            var expected = new[] {"Foo", "BarBaz"};

            // ACT
            string[] actual = input.SplitAtIndices(indices, StringSplitOptions.None);

            // ASSERT
            actual.ShouldBeEquivalentTo(expected);
        }

        [TestMethod, TestCategory("Unit")]
        public void SplitAtIndices_WithNoIndices_ReturnsOriginalString()
        {
            // ARRANGE
            const string input = "FooBarBaz";
            var indices = new int[0];

            // ACT
            string[] actual = input.SplitAtIndices(indices, StringSplitOptions.None);

            // ASSERT
            actual.ShouldBeEquivalentTo(new[] {input});
        }
    }
}