using System;
using System.Net.Http;
using FluentAssertions;
using Knapcode.KitchenSink.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Knapcode.KitchenSink.Tests.Support
{
    [TestClass]
    public class GuardTests
    {
        [TestMethod, TestCategory("Unit")]
        public void ArgumentNotNull_WithValidInput_Passes()
        {
            // ACT
            Action action = () => Guard.ArgumentNotNull(new HttpRequestMessage(), "request");

            // ASSERT
            action.ShouldNotThrow();
        }

        [TestMethod, TestCategory("Unit")]
        public void ArgumentNotNull_WithNullInput_ThrowsArgumentNullException()
        {
            // ACT
            Action action = () => Guard.ArgumentNotNull((HttpRequestMessage)null, "request");

            // ASSERT
            action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("request");
        }
    }
}
