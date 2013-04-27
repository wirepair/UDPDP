using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace UDPDPLogWriter
{
    /*
    public class LogEvent
    {
        public byte[] data;
        public int sender;
        public int count;
    }
    */
    [Serializable]
    public class Transmissions
    {
        public List<LogEvent> LogEventList = new List<LogEvent>();
    }

    [Serializable]
    public class LogEvent
    {
        public byte[] data;

        public int sender;

        public int count;
    }

    public interface IULogWriter 
    {
        int Open(string filename, string mode);

        void LogTransmission(byte[] buffer, string sender, int count);

        Transmissions ReadTransmission();

        void Close();

    }
}
