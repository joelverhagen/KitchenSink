namespace Knapcode.KitchenSink.Hashing
{
    public interface IBucketKeyMapper
    {
        long GetKey(string key);
    }
}
