using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTDReaderLibrary
{
    class Pointer
    {
        private byte[] content;
        private int Length;
        private int cursor;

        public Pointer(byte[] content, int length)
        {
            this.content = content;
            Length = length;
            cursor = 0;
        }

        public void moveRight(int bytes)
        {
            cursor += bytes;
        }

        public int getPosition()
        {
            return cursor;
        }

        public byte[] getData()
        {
            return content;
        }
    }
}
