using SearchEngine_.indexing.api;

namespace SearchEngine_.indexing.impl
{
    public class InvertedIndexStorageFactory: IInvertedIndexStorageFactory
    {
        private static IInvertedIndexStorage _storage = new MySqlBasedInvertedTokenStorage();
        public  IInvertedIndexStorage Create()
        {
            return _storage;
        }
    }
}
