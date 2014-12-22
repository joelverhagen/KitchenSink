namespace Knapcode.KitchenSink.Hashing
{
    public class StringBucketAlgorithm : IStringBucketAlgorithm
    {
        private readonly IBucketAlgorithm _bucketAlgorithm;
        private readonly IBucketKeyMapper _bucketKeyMapper;

        public StringBucketAlgorithm(IBucketKeyMapper bucketKeyMapper, IBucketAlgorithm bucketAlgorithm)
        {
            _bucketKeyMapper = bucketKeyMapper;
            _bucketAlgorithm = bucketAlgorithm;
        }

        public int GetBucket(string key)
        {
            return _bucketAlgorithm.GetBucket(_bucketKeyMapper.GetKey(key));
        }
    }
}