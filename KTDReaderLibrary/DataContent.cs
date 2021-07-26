namespace KTDReaderLibrary
{
    public partial class KtdReader
    {
        private class DataContent
        {
            public byte[] content;
            private long length;

            public DataContent(byte[] content, int length)
            {
                this.content = content;
                this.length = length;
            }

            public byte[] getContent()
            {
                return content;
            }

            public long getLength()
            {
                return length;
            }
        }
    }
}
