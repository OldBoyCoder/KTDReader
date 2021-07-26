using System;
using System.IO;

namespace KTDReaderLibrary
{
    internal class ByteArrayInputStream:MemoryStream
    {
        public ByteArrayInputStream(byte[] buffer) : base(buffer)
        {
        }

        public ByteArrayInputStream(byte[] buffer, int index, int count) : base(buffer, index, count)
        {
        }

        public int read()
        {
            return ReadByte();
        }

        internal byte[] read(int v1, int v2)
        {
            var buffer = new byte[v2-v1];
            Read(buffer, 0, v2 - v1 );
            return buffer;
        }

        public bool EndOfStream()
        {
            if (Position >= Length) return true;
            return false;
        }
    }
}