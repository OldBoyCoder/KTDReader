namespace KTDReaderLibrary
{
    static class DataTransformations
    {

        public static int ReadUnsignedShortReverse(byte[] b, int position)
        {
            int tempInt = 0;
            tempInt += b[position] & 0xFF;
            return tempInt + ((b[position + 1] << 8) & 0xFF00);
        }
    }
}
