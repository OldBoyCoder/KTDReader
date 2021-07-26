namespace KTDReaderLibrary
{
    public partial class KtdReader
    {
        private class DataContent
        {
            internal readonly byte[] Content;
            internal readonly long Length;

            public DataContent(byte[] content, int length)
            {
                Content = content;
                Length = length;
            }
        }
    }
}
