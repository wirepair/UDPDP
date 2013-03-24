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

namespace UDPDP
{
    class UdpProxyClient : UDPClientService
    {
        const int SERVER = 0;
        ProxyUDPSocketService _puss = null;
        StateObject _saved_client_so = null;
        // set's a reference of the server so we can send data back on ProcessBuffer
        public UdpProxyClient(IPEndPoint iep, ProxyUDPSocketService puss)
            : base(iep)
        {
            _puss = puss;
        }
        // we need endpoint and socket of the proxy_server <-> client
        public void SetRealClientState(StateObject so)
        {
            _saved_client_so = so;
        }

        public override void ProcessBuffer(StateObject state)
        {
            StateObject real_client_so = new StateObject();
            Console.WriteLine("Real server responded with " + state.sb.ToString());
            Console.WriteLine("Forwarding it back to real client.");
            real_client_so.workSocket = _saved_client_so.workSocket;
            real_client_so.endPoint = _saved_client_so.endPoint;
            // this is the data the real server sent us, which we need to return to the real client.
            real_client_so.buffer = state.buffer;
            // resize it to just recv'd() bytes.
            Array.Resize(ref real_client_so.buffer, state.recvSize);

            int count = _puss.IncrementServerCount();
            _puss.LogData(real_client_so.buffer, "server", count);

            // fire off data back to the real client.
            _puss.SendData(real_client_so, new AsyncCallback(_puss.OnSent));
        }
    }

    class ProxyUDPSocketService : UDPServerService
    {
        #region Properties
        protected IPEndPoint _dest = null;
        protected IPEndPoint _src = null;
        protected StateObject _initialRemoteSO = null;
        protected StateObject _serverSO = null;
        protected FileStream _outputStream = null;
        protected StreamWriter _writer = null;
        protected bool _modify = false;
        protected string _format = null;
        protected string _decrypt = null;
        protected int serverCount = 0;
        protected int clientCount = 0;
        protected UdpProxyClient upc;
        protected IDecryptor Decrypt = null;

        const int CLIENT = 1;
        
        #endregion

        #region Constructors
        public ProxyUDPSocketService(IPEndPoint src, IPEndPoint dest)
            : base(src)
        {
            _src = src;
            _dest = dest;
        }
        public ProxyUDPSocketService(IPEndPoint src, IPEndPoint dest, string output, string format, bool modify)
            : base(src)
        {
            _src = src;
            _dest = dest;
            try
            {
                _outputStream = new FileStream(output, FileMode.Create);
                _format = format;
                _writer = new StreamWriter(_outputStream);
                _modify = modify;
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

            if (_outputStream != null && _writer != null)
            {
                string header = string.Format("\r\n===={0}.{1}====\r\n", sender, count);
                _writer.Write(header);
                _writer.Flush();
                if (_format.Equals("raw"))
                {
                    // we have raw bytes here so just use the stream instead of the writer.
                    _outputStream.Write(buffer, 0, buffer.Length);
                    //_writer.Write(buffer);
                }
                else if (_format.Equals("asciibin"))
                {
                    LogHexBuffer(_writer, buffer);
                }
            }
            _writer.Flush();
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
        #endregion

        #region Socket Related Code
        public bool ConnectToServer()
        {
            upc = new UdpProxyClient(_dest, this);
            _initialRemoteSO = upc.Connect();
            if (_initialRemoteSO == null)
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
            _initialRemoteSO.buffer = state.buffer;
            // resize the buffer down to only the bytes we got.
            Array.Resize(ref _initialRemoteSO.buffer, state.recvSize);
            int count = IncrementClientCount();
            LogData(_initialRemoteSO.buffer, "client", count);

            if (Decrypt != null)
            {
                byte[] decrypted = Decrypt.Decrypt(CLIENT, _initialRemoteSO.buffer, state.recvSize, count);
                LogData(decrypted, "client.decrypted", count);
                if (_modify == true)
                {
                    Console.WriteLine("Modified data!");
                    Console.WriteLine("x: {0}", Encoding.ASCII.GetString(decrypted));
                    _initialRemoteSO.buffer = decrypted;
                }
            }
            // set the state of the real client for our proxy client.
            upc.SetRealClientState(state);

            // send the data the real client sent us to the real server (either modified or not).
            upc.SendData(_initialRemoteSO, new AsyncCallback(upc.OnSent));
        }
        #endregion
    }
}
