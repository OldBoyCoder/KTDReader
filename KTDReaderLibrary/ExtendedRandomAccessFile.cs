using System.IO;

namespace KTDReaderLibrary
{
    class ExtendedRandomAccessFile
    {
        private BinaryReader br;
        private string filename;

        public ExtendedRandomAccessFile(string filename)
        {
            br = new BinaryReader(File.Open(filename, FileMode.Open));
            this.filename = filename;
        }

        public byte[] read(int length)
        {
            return br.ReadBytes(length);
        }

        public byte readUnsignedByte()
        {
            return br.ReadByte();
        }

        public void seek(long l)
        {
            br.BaseStream.Seek(l, SeekOrigin.Begin);
        }

        public long readUnsignedIntReverse()
        {
            return br.ReadUInt32();
        }

        public long readUnsignedShortReverse()
        {
            return br.ReadUInt16();
        }

        public void close()
        {
            br.Close();
        }
    }
}
