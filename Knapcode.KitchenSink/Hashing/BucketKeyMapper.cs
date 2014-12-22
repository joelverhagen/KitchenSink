using System;
using System.Text;
using Murmur;

namespace Knapcode.KitchenSink.Hashing
{
    public class BucketKeyMapper : IBucketKeyMapper
    {
        public long GetKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            using (var hashAlgorithm = MurmurHash.Create128())
            {
                byte[] keyBuffer = Encoding.UTF8.GetBytes(key);
                var hash = hashAlgorithm.ComputeHash(keyBuffer);
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