namespace KTDReaderLibrary
{
    public partial class KtdReader
    {
        private class DataContent
        {
            internal byte[] Content;
            internal long Length;

            public DataContent(byte[] content, int length)
            {
                Content = content;
                Length = length;
            }
        }
    }
}
