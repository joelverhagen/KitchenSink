using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Knapcode.KitchenSink.Hashing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Knapcode.KitchenSink.Tests.Hashing
{
    [TestClass]
    public class ConsistentHashingAlgorithmTests
    {
        [TestMethod, TestCategory("Unit")]
        public void Constructor_WithZeroBucketCount_ThrowsException()
        {
            // ARRANGE, ACT
            Action action = () => new ConsistentHashingAlgorithm(0, 1024);

            // ASSERT
            var expectation = action.ShouldThrow<ArgumentOutOfRangeException>();
            expectation.And.ParamName.Should().Be("bucketCount");
            expectation.And.ActualValue.Should().Be(0);
        }

        [TestMethod, TestCategory("Unit")]
        public void Constructor_WithZeroReplicaCount_ThrowsException()
        {
            // ARRANGE, ACT
            Action action = () => new ConsistentHashingAlgorithm(8, 0);

            // ASSERT
            var expectation = action.ShouldThrow<ArgumentOutOfRangeException>();
            expectation.And.ParamName.Should().Be("replicaCount");
            expectation.And.ActualValue.Should().Be(0);
        }

        [TestMethod, TestCategory("Unit")]
        public void Constructor_WithValidParameters_Succeeds()
        {
            // ARRANGE, ACT
            Action action = () => new ConsistentHashingAlgorithm(8, 1024);

            // ASSERT
            action.ShouldNotThrow();
        }

        [TestMethod, TestCategory("Unit")]
        public void GetBucket_WithSameKey_ReturnsSameBucket()
        {
            // ARRANGE
            var cha = new ConsistentHashingAlgorithm(8, 1024);
            const long key = 42;
            
            // ACT
            var bucketA = cha.GetBucket(key);
            var bucketB = cha.GetBucket(key);

            // ASSERT
            bucketA.Should().Be(bucketB);
        }

        [TestMethod, TestCategory("Unit")]
        public void BucketCount_Always_ReturnsConstructorValue()
        {
            // ARRANGE
            const int bucketCount = 8;
            var cha = new ConsistentHashingAlgorithm(bucketCount, 1024);

            // ACT
            int actual = cha.BucketCount;

            // ASSERT
            actual.Should().Be(bucketCount);
        }

        [TestMethod, TestCategory("Unit")]
        public void ReplicaCount_Always_ReturnsConstructorValue()
        {
            // ARRANGE
            const int replicaCount = 1024;
            var cha = new ConsistentHashingAlgorithm(8, replicaCount);

            // ACT
            int actual = cha.ReplicaCount;

            // ASSERT
            actual.Should().Be(replicaCount);
        }

        [TestMethod, TestCategory("Unit")]
        public void GetBucket_WithRandomInputAnd2Buckets_IsApproximatelyEvenlyDistributed()
        {
            GetBucket_WithRandomInput_IsApproximatelyEvenlyDistributed(2);
        }

        [TestMethod, TestCategory("Unit")]
        public void GetBucket_WithRandomInputAnd4Buckets_IsApproximatelyEvenlyDistributed()
        {
            GetBucket_WithRandomInput_IsApproximatelyEvenlyDistributed(4);
        }

        [TestMethod, TestCategory("Unit")]
        public void GetBucket_WithRandomInputAnd8Buckets_IsApproximatelyEvenlyDistributed()
        {
            GetBucket_WithRandomInput_IsApproximatelyEvenlyDistributed(8);
        }

        [TestMethod, TestCategory("Unit")]
        public void GetBucket_WithRandomInputAnd16Buckets_IsApproximatelyEvenlyDistributed()
        {
            GetBucket_WithRandomInput_IsApproximatelyEvenlyDistributed(16);
        }

        [TestMethod, TestCategory("Unit")]
        public void GetBucket_WithRandomInputAnd32Buckets_IsApproximatelyEvenlyDistributed()
        {
            GetBucket_WithRandomInput_IsApproximatelyEvenlyDistributed(32);
        }

        private static void GetBucket_WithRandomInput_IsApproximatelyEvenlyDistributed(int bucketCount)
        {
            // ARRANGE
            const int iterations = 10000;
            Dictionary<int, int> counts = Enumerable.Range(0, bucketCount).ToDictionary(i => i, i => 0);
            var cha = new ConsistentHashingAlgorithm(bucketCount, 1024);
            var random = new Random(0);
            var longBuffer = new byte[8];

            double expectedCount = iterations/(double) bucketCount;
            double maximumDifference = 0.15 * expectedCount;

            // ACT
            for (int i = 0; i < iterations; i++)
            {
                random.NextBytes(longBuffer);
                long key = BitConverter.ToInt64(longBuffer, 0);
                int bucket = cha.GetBucket(key);
                counts[bucket]++;
            }
            
            // ASSERT
            Console.WriteLine("Bucket count: {0}. Expected count: {1}. Maximum difference: {2}.", bucketCount, expectedCount, maximumDifference);
            for (int i = 0; i < bucketCount; i++)
            {
                Console.WriteLine("Bucket: {0}. Actual count: {1}. Difference: {2}.", i, counts[i], Math.Abs(expectedCount - counts[i]));
                ((double) counts[i]).Should().BeApproximately(expectedCount, maximumDifference);
            }
        }
    }
}
