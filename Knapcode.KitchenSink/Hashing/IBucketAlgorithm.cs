namespace Knapcode.KitchenSink.Hashing
{
    public interface IBucketAlgorithm
    {
        int BucketCount { get; }
        int GetBucket(long key);
    }
}