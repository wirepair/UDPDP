using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;


namespace UDPDPLogWriter
{
    public class JSONLogWriter : IULogWriter
    {
        const int SERVER = 0;
        const int CLIENT = 1;

        protected FileStream stream = null;
        protected DataContractJsonSerializer ser = null;
        protected int position = 0;

        public JSONLogWriter()
        { }

        public int Open(string filename, string mode)
        {
            try
            {
                if (mode.Equals("r"))
                {
                    stream = new FileStream(filename, FileMode.Open);
                    ser = new DataContractJsonSerializer(typeof(LogEvent));
                }
                else
                {
                    stream = new FileStream(filename, FileMode.Create);
                    ser = new DataContractJsonSerializer(typeof(LogEvent));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return -1;
            }
            return 0;
        }

        public void LogTransmission(byte[] buffer, string sender, int count)
        {
            LogEvent log = new LogEvent();
            log.count = count;
            if (sender.Equals("client"))
            {
                log.sender = CLIENT;
            }
            else 
            {
                log.sender = SERVER;
            }
            log.data = buffer;
            ser.WriteObject(stream, log);
        }

        public LogEvent ReadTransmission()
        {
            return (LogEvent)ser.ReadObject(stream);
        }

        public void Close()
        {
            stream.Close();
        }
    }
}
