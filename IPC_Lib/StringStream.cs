namespace IPC_Lib
{
    using System.IO.Pipes;
    using System.Text;


    /// <summary>
    /// Помощен клас за четене и писане на символни низове през PipeStream
    /// </summary>
    public class StringStream
    {
        private PipeStream ioStream;
        private UnicodeEncoding streamEncoding;

        public StringStream(PipeStream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            int len = 0;
            len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            if (len < 0)
            {
                throw new EndOfStreamException();
            }
            byte[] inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);
            return streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }

}