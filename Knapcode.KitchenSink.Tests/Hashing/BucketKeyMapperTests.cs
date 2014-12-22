using System;
using FluentAssertions;
using Knapcode.KitchenSink.Hashing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Knapcode.KitchenSink.Tests.Hashing
{
    [TestClass]
    public class BucketKeyMapperTests
    {
        [TestMethod, TestCategory("Unit")]
        public void GetKey_WithNull_ThrowsException()
        {
            // ARRANGE
            var bkm = new BucketKeyMapper();

            // ACT
            Action action = () => bkm.GetKey(null);

            // ASSERT
            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("key");
        }

        [TestMethod, TestCategory("Unit")]
        public void GetKey_WithSameInput_ReturnsSameKey()
        {
            // ARRANGE
            var bkm = new BucketKeyMapper();
            const string key = "foo";

            // ACT
            var keyA = bkm.GetKey(key);
            var keyB = bkm.GetKey(key);

            // ASSERT
            keyA.Should().Be(keyB);
        }
    }
}
