namespace Knapcode.KitchenSink.Hashing
{
    public interface IStringBucketAlgorithm
    {
        int GetBucket(string key);
    }
}