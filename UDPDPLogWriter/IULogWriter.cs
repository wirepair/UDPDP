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
   

    [DataContract]
    public class LogEvent
    {
        [DataMember]
        internal byte[] data;

        [DataMember]
        internal int sender;

        [DataMember]
        internal int count;
    }

    public interface IULogWriter 
    {
        int Open(string filename, string mode);

        void LogTransmission(byte[] buffer, string sender, int count);

        void Close();

    }
}
