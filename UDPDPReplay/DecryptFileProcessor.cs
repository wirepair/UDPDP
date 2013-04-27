using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Decryptor;
using UDPDPLogWriter;
using System.Runtime.InteropServices;
// author @_wirepair : github.com/wirepair
// date: 04272013 
// copyright: ME AND MINE but i guess you can use it :D.
namespace UDPPDReplay
{

    class DecryptOptions
    {
        public string dll;
        public string input;
        public string output;
        public string format;
        public string outputformat;
    }

    class DecryptFileProcessor
    {
        protected int SERVER = 0;
        protected int CLIENT = 1;
        protected DecryptOptions decrypto;
        protected IDecryptor Decrypt = null;
        protected IULogWriter reader = null;
        protected IULogWriter writer = null;
        
        public DecryptFileProcessor(DecryptOptions decrypto)
        {
            this.decrypto = decrypto;
            if (!this.decrypto.output.Equals(""))
            {
                if (this.decrypto.outputformat.Equals("json"))
                {
                    writer = new JSONLogWriter();
                }
                else if (this.decrypto.outputformat.Equals("asciibin"))
                {
                    writer = new HexLogWriter();
                }
                else
                {
                    Console.WriteLine("Error only asciibin and json supported!");
                    return;
                }
                writer.Open(this.decrypto.output, "w");
            }
        }
        
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
  
        public int ProcessData()
        {
            int ret = SetDecryptor(decrypto.dll);
            if (ret != 0)
            {
                return ret;
            }
            if (decrypto.format.Equals("json"))
            {
                var output = new IntPtr();
                var output_size = 0;
                string sender = "";
                reader = new JSONLogWriter();
                reader.Open(decrypto.input, "r");
                Transmissions trans = reader.ReadTransmission();
                foreach (LogEvent e in trans.LogEventList)
                {
                    try
                    {
                        if (e.sender == CLIENT)
                        {
                            sender = "client";
                        }
                        else
                        {
                            sender = "server";
                        }
                        Console.WriteLine("sender: {0} packet: {1} length: {2}", sender, e.count, e.data.Length);
                        Decrypt.Decrypt(e.sender, e.data, e.data.Length, e.count, ref output, ref output_size);
                        byte[] decrypted = new byte[output_size];
                        Marshal.Copy(output, decrypted, 0, output_size);
                        Console.WriteLine("\nAfter decrypt: {0}", Encoding.ASCII.GetString(decrypted));
                        Console.WriteLine("--------------------------------------");
                        if (writer != null)
                        {
                            writer.LogTransmission(decrypted, sender, e.count);
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
            }
            
            return 0;
        }
    }
}
