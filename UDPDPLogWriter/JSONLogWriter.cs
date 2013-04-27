using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

// author @_wirepair : github.com/wirepair
// date: 04272013 
// copyright: ME AND MINE but i guess you can use it :D.
namespace UDPDPLogWriter
{
    public class JSONLogWriter : IULogWriter
    {
        const int SERVER = 0;
        const int CLIENT = 1;

        protected FileStream stream = null;
        protected DataContractJsonSerializer ser = null;
        protected Transmissions trans = new Transmissions();
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
                    ser = new DataContractJsonSerializer(typeof(Transmissions));
                }
                else
                {
                    stream = new FileStream(filename, FileMode.Create);
                    ser = new DataContractJsonSerializer(typeof(Transmissions));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return -1;
            }
            return 0;
        }

        /**
         * A really terribly way of logging, basically we append each event
         * to our Transmissions object then write out it's entire contents
         * every time. Depending on how much traffic is actually being sent,
         * it may be necessary to write out each transmission to its own
         * file.
         **/
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
            trans.LogEventList.Add(log);
            stream.Position = 0; // overwrite everything
            ser.WriteObject(stream, trans);
        }

        public Transmissions ReadTransmission()
        {
            return (Transmissions)ser.ReadObject(stream);
        }

        public void Close()
        {
            stream.Close();
        }
    }
}
