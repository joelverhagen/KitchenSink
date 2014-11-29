using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentAssertions;
using Knapcode.KitchenSink.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Knapcode.KitchenSink.Tests.Support
{
    [TestClass]
    public class Base64UrlTests
    {
        private const string AllBytesEncoded = "AAECAwQFBgcICQoLDA0ODxAREhMUFRYXGBkaGxwdHh8gISIjJCUmJygpKissLS4vMDEy" +
                                               "MzQ1Njc4OTo7PD0-P0BBQkNERUZHSElKS0xNTk9QUVJTVFVWV1hZWltcXV5fYGFiY2Rl" +
                                               "ZmdoaWprbG1ub3BxcnN0dXZ3eHl6e3x9fn-AgYKDhIWGh4iJiouMjY6PkJGSk5SVlpeY" +
                                               "mZqbnJ2en6ChoqOkpaanqKmqq6ytrq-wsbKztLW2t7i5uru8vb6_wMHCw8TFxsfIycrL" +
                                               "zM3Oz9DR0tPU1dbX2Nna29zd3t_g4eLj5OXm5-jp6uvs7e7v8PHy8_T19vf4-fr7_P3-" +
                                               "_w";

        private static readonly byte[] AllBytesDecoded = Enumerable
            .Range(byte.MinValue, byte.MaxValue - byte.MinValue + 1)
            .Select(b => (byte) b)
            .ToArray();

        [TestMethod, TestCategory("Unit")]
        public void Encode_WithAllPossibleBytes_ReturnsUrlSafeString()
        {
            // ARRANGE
            string base64 = Base64Url.Encode(AllBytesDecoded);

            // ACT
            string urlEncodedBase64 = HttpUtility.UrlEncode(base64);

            // ASSERT
            base64.ShouldBeEquivalentTo(urlEncodedBase64);
        }

        [TestMethod, TestCategory("Unit")]
        public void Encode_WithAllPossibleBytes_EncodesExpected()
        {
            // ACT
            string base64 = Base64Url.Encode(AllBytesDecoded);

            // ASSERT
            base64.ShouldBeEquivalentTo(AllBytesEncoded);
        }

        [TestMethod, TestCategory("Unit")]
        public void Decode_WithAllPossibleBytes_DecodesExpected()
        {
            // ACT
            byte[] bytes = Base64Url.Decode(AllBytesEncoded);

            // ASSERT
            bytes.ShouldAllBeEquivalentTo(AllBytesDecoded);
        }

        [TestMethod, TestCategory("Unit")]
        public void Encode_WithEmptyArray_ReturnsEmptyArray()
        {
            // ACT
            string base64 = Base64Url.Encode(new byte[0]);

            // ASSERT
            base64.Should().BeEmpty();
        }

        [TestMethod, TestCategory("Unit")]
        public void Encode_WithNullArray_ThrowException()
        {
            // ACT
            Action a = () => Base64Url.Encode(null);

            // ASSERT
            a.ShouldThrow<ArgumentNullException>().And.ParamName.ShouldBeEquivalentTo("bytes");
        }

        [TestMethod, TestCategory("Unit")]
        public void Decode_WithNullString_ThrowException()
        {
            // ACT
            Action a = () => Base64Url.Decode(null);

            // ASSERT
            a.ShouldThrow<ArgumentNullException>().And.ParamName.ShouldBeEquivalentTo("base64");
        }

        [TestMethod, TestCategory("Unit")]
        public void Decode_WithMissingTwoOutOfTwoPadding_Works()
        {
            VerifyDecode("QQ", new[] {(byte) 'A'});
        }

        [TestMethod, TestCategory("Unit")]
        public void Decode_WithMissingOneOutOfTwoPadding_Works()
        {
            VerifyDecode("QQ=", new[] {(byte) 'A'});
        }

        [TestMethod, TestCategory("Unit")]
        public void Decode_WithMissingZeroOutOfTwoPadding_Works()
        {
            VerifyDecode("QQ==", new[] {(byte) 'A'});
        }

        [TestMethod, TestCategory("Unit")]
        public void Decode_WithExtraPadding_Works()
        {
            VerifyDecode("QQ===", new[] {(byte) 'A'});
        }

        [TestMethod, TestCategory("Unit")]
        public void Decode_WithMissingOneOutOfOnePadding_Works()
        {
            VerifyDecode("QUI", new[] {(byte) 'A', (byte) 'B'});
        }

        [TestMethod, TestCategory("Unit")]
        public void Decode_WithMissingZeroOutOfOnePadding_Works()
        {
            VerifyDecode("QUI=", new[] {(byte) 'A', (byte) 'B'});
        }

        [TestMethod, TestCategory("Unit")]
        public void Decode_WithMissingZeroOutOfZeroPadding_Works()
        {
            VerifyDecode("QUJD", new[] {(byte) 'A', (byte) 'B', (byte) 'C'});
        }

        [TestMethod, TestCategory("Unit")]
        public void Encode_WithTwoStrippedPadding_Works()
        {
            VerifyEncode(new[] {(byte) 'A'}, "QQ");
        }

        [TestMethod, TestCategory("Unit")]
        public void Encode_WithOneStrippedPadding_Works()
        {
            VerifyEncode(new[] {(byte) 'A', (byte) 'B'}, "QUI");
        }

        [TestMethod, TestCategory("Unit")]
        public void Encode_WithZeroStrippedPadding_Works()
        {
            VerifyEncode(new[] {(byte) 'A', (byte) 'B', (byte) 'C'}, "QUJD");
        }

        private static void VerifyEncode(byte[] input, string expected)
        {
            // ACT
            string actual = Base64Url.Encode(input);

            // ASSERT
            actual.ShouldBeEquivalentTo(expected);
        }

        private static void VerifyDecode(string input, IEnumerable<byte> expected)
        {
            // ACT
            byte[] actual = Base64Url.Decode(input);

            // ASSERT
            actual.ShouldAllBeEquivalentTo(expected);
        }
    }
}