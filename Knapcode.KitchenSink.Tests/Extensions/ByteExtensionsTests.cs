using System;
using FluentAssertions;
using Knapcode.KitchenSink.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Knapcode.KitchenSink.Tests.Extensions
{
    [TestClass]
    public class ByteExtensionsTests
    {
        [TestMethod, TestCategory("Unit")]
        public void GetBit_WithFalseBit_ReturnsFalse()
        {
            // ARRANGE
            const byte b = 0;

            // ACT
            bool bit = b.GetBit(0);

            // ASSERT
            bit.Should().BeFalse();
        }

        [TestMethod, TestCategory("Unit")]
        public void GetBit_WithTrueBit_ReturnsTrue()
        {
            // ARRANGE
            const byte b = 1;

            // ACT
            bool bit = b.GetBit(0);

            // ASSERT
            bit.Should().BeTrue();
        }

        [TestMethod, TestCategory("Unit")]
        public void SetBit_WithTrueBit_SetsTrue()
        {
            // ARRANGE
            byte b = 0;

            // ACT
            b = b.SetBit(0, true);

            // ASSERT
            b.GetBit(0).Should().BeTrue();
        }

        [TestMethod, TestCategory("Unit")]
        public void SetBit_WithFalseBit_SetsFalse()
        {
            // ARRANGE
            byte b = 1;

            // ACT
            b = b.SetBit(0, false);

            // ASSERT
            b.GetBit(0).Should().BeFalse();
        }

        [TestMethod, TestCategory("Unit")]
        public void SetBit_WithNegativeIndex_ThrowsException()
        {
            // ARRANGE, ACT
            Action a = () => byte.MinValue.SetBit(-1, true);

            // ASSERT
            a.ShouldThrow<ArgumentOutOfRangeException>().And.ParamName.Should().Be("index");
        }

        [TestMethod, TestCategory("Unit")]
        public void SetBit_WithTooLargeIndex_ThrowsException()
        {
            // ARRANGE, ACT
            Action a = () => byte.MinValue.SetBit(8, true);

            // ASSERT
            a.ShouldThrow<ArgumentOutOfRangeException>().And.ParamName.Should().Be("index");
        }

        [TestMethod, TestCategory("Unit")]
        public void GetBit_WithNegativeIndex_ThrowsException()
        {
            // ARRANGE, ACT
            Action a = () => byte.MinValue.GetBit(-1);

            // ASSERT
            a.ShouldThrow<ArgumentOutOfRangeException>().And.ParamName.Should().Be("index");
        }

        [TestMethod, TestCategory("Unit")]
        public void GetBit_WithTooLargeIndex_ThrowsException()
        {
            // ARRANGE, ACT
            Action a = () => byte.MinValue.GetBit(8);

            // ASSERT
            a.ShouldThrow<ArgumentOutOfRangeException>().And.ParamName.Should().Be("index");
        }
    }
}
