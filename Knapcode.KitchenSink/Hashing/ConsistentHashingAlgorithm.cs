using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Murmur;

namespace Knapcode.KitchenSink.Hashing
{
    public class ConsistentHashingAlgorithm : IBucketAlgorithm
    {
        private static readonly ConcurrentDictionary<Tuple<int, int>, Ring> Rings = new ConcurrentDictionary<Tuple<int, int>, Ring>();

        private readonly Ring _ring;

        public ConsistentHashingAlgorithm(int bucketCount, int replicaCount)
        {
            if (bucketCount <= 0)
            {
                throw new ArgumentOutOfRangeException("bucketCount", bucketCount, "The bucket count must be greater than 0.");
            }

            if (replicaCount <= 0)
            {
                throw new ArgumentOutOfRangeException("replicaCount", replicaCount, "The replica count must be greater than 0.");
            }

            BucketCount = bucketCount;
            ReplicaCount = replicaCount;

            _ring = Rings.GetOrAdd(new Tuple<int, int>(BucketCount, ReplicaCount), t => new Ring(t.Item1, t.Item2));
        }

        public int ReplicaCount { get; private set; }
        public int BucketCount { get; private set; }

        public int GetBucket(long key)
        {
            return _ring.GetBucket(key);
        }

        private class Ring
        {
            private readonly long[] _boundKeys;
            private readonly Dictionary<long, int> _bounds;

            public Ring(int bucketCount, int replicaCount)
            {
                var bounds = new Dictionary<long, int>(bucketCount*replicaCount);

                var keyBuffer = new byte[8];
                for (int bucketIndex = 0; bucketIndex < bucketCount; bucketIndex++)
                {
                    for (int replicaIndex = 0; replicaIndex < replicaCount; replicaIndex++)
                    {
                        Buffer.BlockCopy(BitConverter.GetBytes(bucketIndex), 0, keyBuffer, 0, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(replicaIndex), 0, keyBuffer, 4, 4);
                        long key = BitConverter.ToInt64(keyBuffer, 0);
                        long ringPosition = GetRingPosition(key);
                        bounds[ringPosition] = bucketIndex;
                    }
                }

                _bounds = bounds;
                _boundKeys = _bounds.Keys.OrderBy(k => k).ToArray();
            }

            public int GetBucket(long key)
            {
                long ringPosition = GetRingPosition(key);
                int index = Array.BinarySearch(_boundKeys, ringPosition);
                if (index < 0)
                {
                    index = (~index)%_boundKeys.Length;
                }

                return _bounds[_boundKeys[index]];
            }

            private long GetRingPosition(long key)
            {
                using (Murmur128 hashAlgorithm = MurmurHash.Create128())
                {
                    byte[] hash = hashAlgorithm.ComputeHash(BitConverter.GetBytes(key));
                    long total = 0;
                    for (int startIndex = 0; startIndex < hash.Length; startIndex += 8)
                    {
                        unchecked
                        {
                            total += BitConverter.ToInt64(hash, startIndex);
                        }
                    }

                    return total;
                }
            }
        }
    }
}