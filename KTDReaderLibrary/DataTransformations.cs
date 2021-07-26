using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace KTDReaderLibrary
{
    class DataTransformations
    {

        public static int readUnsignedShortReverse(byte[] b, int position)
        {
            int tempInt = 0;
            tempInt += b[position] & 0xFF;
            return tempInt += b[position + 1] << 8 & 0xFF00;
        }
    }
}
