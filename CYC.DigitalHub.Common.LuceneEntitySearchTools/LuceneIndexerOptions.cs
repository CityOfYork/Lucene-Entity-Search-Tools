namespace CYC.DigitalHub.Common.LuceneEntitySearchTools
{
    /// <summary>
    /// Class to provide the neccessary parameters for constructing a new LuceneIndexer
    /// </summary>
    public class LuceneIndexerOptions
    {
        public bool UseRamDirectory { get; protected set; }
        public string Path { get; protected set; }
        public int? MaximumFieldLength { get; protected set; }

        public LuceneIndexerOptions(
            string path,
            int? maximumFieldLength = null,
            bool useRamDirectory = false)
        {
            Path = path;
            MaximumFieldLength = maximumFieldLength;
            UseRamDirectory = useRamDirectory;
        }
    }
}
