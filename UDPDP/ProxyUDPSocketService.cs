using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.IO;
using System.Text;
using SocketService;
using Decryptor;
using UDPDPLogWriter;

// author @_wirepair : github.com/wirepair
// date: 04272013 
// copyright: ME AND MINE but i guess you can use it :D.
namespace UDPDP
{
    // The UDP proxy client code
    class UdpProxyClient : UDPClientService
    {
        const int SERVER = 0;
        ProxyUDPSocketService puss = null;
        StateObject saved_client_so = null;
        // sets a reference of the server so we can send data back on ProcessBuffer
        public UdpProxyClient(IPEndPoint iep, ProxyUDPSocketService puss)
            : base(iep)
        {
            this.puss = puss;
        }
        // we need endpoint and socket of the proxy_server <-> client
        public void SetRealClientState(StateObject so)
        {
            saved_client_so = so;
        }

        public override void ProcessBuffer(StateObject state)
        {
            StateObject real_client_so = new StateObject();
            Console.WriteLine("Real server responded with " + state.sb.ToString());
            Console.WriteLine("Forwarding it back to real client.");
            real_client_so.workSocket = saved_client_so.workSocket;
            real_client_so.endPoint = saved_client_so.endPoint;
            // this is the data the real server sent us, which we need to return to the real client.
            real_client_so.buffer = state.buffer;
            // resize it to just recv'd() bytes.
            Array.Resize(ref real_client_so.buffer, state.recvSize);

            int count = puss.IncrementServerCount();
            puss.LogData(real_client_so.buffer, "server", count);

            if (puss.Decrypt != null)
            {
                puss.DecryptData(real_client_so, state, "server", count);
            }
            // fire off data back to the real client.
            puss.SendData(real_client_so, new AsyncCallback(puss.OnSent));
        }
    }
    // The udp proxy server code.
    class ProxyUDPSocketService : UDPServerService
    {
        #region Properties
        protected IPEndPoint dest = null;
        protected IPEndPoint src = null;
        protected StateObject initialRemoteSO = null;
        protected StateObject serverSO = null;
        protected IULogWriter writer = null;
        protected bool modify = false;
        protected string _format = null;
        protected string _decrypt = null;
        protected int serverCount = 0;
        protected int clientCount = 0;
        protected UdpProxyClient upc;
        public IDecryptor Decrypt = null;

        const int CLIENT = 1;
        
        #endregion

        #region Constructors
        public ProxyUDPSocketService(IPEndPoint src, IPEndPoint dest)
            : base(src)
        {
            this.src = src;
            this.dest = dest;
        }
        public ProxyUDPSocketService(IPEndPoint src, IPEndPoint dest, string output, string format, bool modify)
            : base(src)
        {
            this.src = src;
            this.dest = dest;
            this.modify = modify;

            try
            {
                if (format.Equals("asciibin"))
                {
                    writer = new HexLogWriter();
                    writer.Open(output, "w");
                }
                else if (format.Equals("json"))
                {
                    writer = new JSONLogWriter();
                    writer.Open(output, "w");
                }
                
            }
            catch (System.IO.IOException ioe)
            {
                Console.WriteLine("Error opening output {0} for writing (logging disabled): {1}", output, ioe.Message);
            }
        }
        #endregion

        public int SetDecryptor(string dll)
        {
            Decrypt = Decryptor.Decryptor.InitDecryptDll(dll);
            if (Decrypt == null)
            {
                Console.WriteLine("Unable to load the decryptor dll!");
                return -1;
            }
            return Decrypt.DecryptInit();
        }

        public void DecryptData(StateObject new_state, StateObject state, string sender, int count)
        {
            IntPtr output = IntPtr.Zero;
            int output_size = 0;
            try
            {
                int ret = Decrypt.Decrypt(CLIENT, new_state.buffer, state.recvSize, count, ref output, ref output_size);
                byte[] decrypted = new byte[output_size];
                Marshal.Copy(output, decrypted, 0, output_size);
                LogData(decrypted, "client", count);
                // if we allow modification, take the response data from our decryption dll.
                if (modify.Equals(true))
                {
                    Console.WriteLine("Modified data!");
                    Console.WriteLine("x: {0}", Encoding.ASCII.GetString(decrypted));
                    new_state.buffer = decrypted;
                    new_state.recvSize = output_size;
                }
            }
            finally
            {
                if (output != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(output);
                }
            }
        }
        #region Logging & Packet Tracking Related Code

        public int IncrementClientCount()
        {
            return clientCount++;
        }

        public int IncrementServerCount()
        {
            return serverCount++;
        }

        public void LogData(byte[] buffer, string sender, int count)
        {
            if (_format != null && _format.Equals("dll"))
            {
                // do dll based logging.
                return;
            }
            writer.LogTransmission(buffer, sender, count);
        }

        
        #endregion

        #region Socket Related Code
        public bool ConnectToServer()
        {
            upc = new UdpProxyClient(dest, this);
            initialRemoteSO = upc.Connect();
            if (initialRemoteSO == null)
            {
                return false;
            }
            return true;
        }

        // for clients that connect to our server
        public override void ProcessBuffer(StateObject state)
        {
            Console.WriteLine("Client connected and sent us: " + state.sb.ToString());
            Console.WriteLine("Forwarding to the real client...");
            initialRemoteSO.buffer = state.buffer;
            // resize the buffer down to only the bytes we got.
            Array.Resize(ref initialRemoteSO.buffer, state.recvSize);
            int count = IncrementClientCount();
            LogData(initialRemoteSO.buffer, "client", count);

            if (Decrypt != null)
            {
                DecryptData(initialRemoteSO, state, "client", count);
            }
            // set the state of the real client for our proxy client.
            upc.SetRealClientState(state);

            // send the data the real client sent us to the real server (either modified or not).
            upc.SendData(initialRemoteSO, new AsyncCallback(upc.OnSent));
        }
        #endregion
    }
}
