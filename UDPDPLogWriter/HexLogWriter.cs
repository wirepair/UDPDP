using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace UDPDPLogWriter
{
    public class HexLogWriter : IULogWriter
    {
        protected FileStream _outputStream = null;
        protected StreamWriter _writer = null;

        public HexLogWriter()
        { }

        public int Open(string filename, string mode)
        {
            try
            {
                if (mode.Equals("r"))
                {
                    throw new NotImplementedException();
                }
                _outputStream = new FileStream(filename, FileMode.Create);
                _writer = new StreamWriter(_outputStream);
            }
            catch (System.IO.IOException ioe)
            {
                Console.WriteLine("Error opening output {0} for writing (logging disabled): {1}", filename, ioe.Message);
                return -1;
            }
            catch (NotImplementedException)
            {
                Console.WriteLine("Open notepad you lazy son of a @#%!");
                return -1;
            }
            return 0;
        }

        public void LogTransmission(byte[] buffer, string sender, int count)
        {
            string header = string.Format("\r\n===={0}.{1}====\r\n", sender, count);
            _writer.Write(header);
            _writer.Flush();
            LogHexBuffer(_writer, buffer);
        }

        public void LogHexBuffer(StreamWriter writer, byte[] buffer)
        {
            int row, column, i = 0;
            int size = buffer.Length;
            for (row = 0; (i + 1) < size; row++)
            {
                // hex
                for (column = 0; column < 16; column++)
                {
                    i = row * 16 + column;
                    if (column == 8)
                    {
                        writer.Write(' ');
                    }

                    if (i < size)
                    {
                        writer.Write(buffer[i].ToString("X2"));
                    }
                    else
                    {
                        writer.Write(' ');
                    }
                    writer.Write(' ');
                }
                // ascii 
                for (column = 0; column < 16; column++)
                {
                    i = row * 16 + column;
                    if (column == 8)
                    {
                        writer.Write(' ');
                    }
                    if (i < size)
                    {
                        if (buffer[i] > 0x20 && buffer[i] < 0x7F)
                        {
                            writer.Write((char)buffer[i]);
                        }
                        else
                        {
                            writer.Write('.');
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                writer.Write("\r\n");
            }

        }

        public void Close()
        {
            _writer.Close();
            _outputStream.Close();
        }
    }
}
