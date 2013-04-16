using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Options;

namespace UDPDP
{
    class Program
    {
       public class UdpProxyOptions
        {
            public string dll = null;
            public string dest = null;
            public string src = null;
            public bool modify = false;
            public string output = null;
            public string format = "asciibin"; // default output format.
            public IPEndPoint src_iep = null;
            public IPEndPoint dest_iep = null;
        }

       static void ShowHelp(OptionSet p, string error)
       {
           if (error != null)
           {
               Console.WriteLine(error);
               Console.WriteLine();
           }
           Console.WriteLine("Usage: UDPDP [OPTIONS]");
           Console.WriteLine("Used proxy and log/decrypt between a UDP client & server.");
           Console.WriteLine("Possible commands:");
           Console.WriteLine();
           p.WriteOptionDescriptions(Console.Out);
           Console.WriteLine("Example:\nUDPDP -s ip.ip.ip.ip:port -d ip.ip.ip.ip:port -o <outfile> --dll=Decryptor.dll");
           Environment.Exit(-1);
       }

        static void ValidateArguments(OptionSet p, UdpProxyOptions upo)
        {
            if (upo.src == null || upo.dest == null)
            {
                ShowHelp(p, "Source *and* destionation are both required.");
            }
            Console.WriteLine(upo.src);
            if (upo.output != null && !(upo.format.Equals("json") || upo.format.Equals("asciibin") || upo.format.Equals("dll")))
            {
                ShowHelp(p, string.Format("Output format is incorrect. {0}...", upo.format));
            }

            upo.src_iep = ParseAddressToEndpoint(upo.src);
            if (upo.src_iep == null)
            {
                ShowHelp(p, "Error invalid source specified.");
            }

            upo.dest_iep = ParseAddressToEndpoint(upo.dest);
            if (upo.dest_iep == null)
            {
                ShowHelp(p, "Error invalid destination specified.");
            }

        }

        static IPEndPoint ParseAddressToEndpoint(string address)
        {
            string[] parts = address.Split(':');
            IPEndPoint iep = null;
            IPAddress ipaddr = null;
            int port = -1;
            if (parts.Length != 2)
            {
                Console.WriteLine("Error : not found to split address and port.");
                return null;
            }
            try
            {
                port = Convert.ToInt32(parts[1]);
            }
            catch (System.FormatException fe)
            {
                Console.WriteLine("Error invalid port specified: {0}", fe.Message);
                return null;
            }

            if (parts[0].Equals("localhost"))
            {
                ipaddr = IPAddress.Parse("127.0.0.1");
            }
            else
            {
                try
                {
                    ipaddr = IPAddress.Parse(parts[0]);
                }
                catch (System.FormatException fe)
                {
                    Console.WriteLine("Error address invalid: {0}", fe.Message);
                    return null;
                }
            }
            iep = new IPEndPoint(ipaddr, port);
            return iep;
        }

        static void StartProxyService(UdpProxyOptions upo)
        {
            ProxyUDPSocketService proxy = null;
            if (upo.output == null)
            {
                proxy = new ProxyUDPSocketService(upo.src_iep, upo.dest_iep);
            }
            else
            {
                try
                {
                    //output_file = File.Open(output, FileMode.Create);
                    proxy = new ProxyUDPSocketService(upo.src_iep, upo.dest_iep, upo.output, upo.format, upo.modify);
                }
                catch (System.IO.IOException ioe)
                {
                    Console.WriteLine("Error opening file {0} for writing: {1}", upo.output, ioe.Message);
                    return;
                }
            }
            if (upo.dll != null)
            {
                Console.WriteLine("Attempting to load decryptor dll {0}", upo.dll);
                int ret = proxy.SetDecryptor(upo.dll);
                if (ret != 0)
                {
                    Console.WriteLine("Error loading dll: {0}\n", ret);
                    return;
                }
            }
            proxy.ConnectToServer();
            proxy.StartListening();
        }
        static void Main(string[] args)
        {
            bool show_help = false;
            UdpProxyOptions upo = new UdpProxyOptions();
            var p = new OptionSet() 
            {
                { "d|dest=", 
                   "destination (to fwd packets to) ip.ip.ip.ip:port.",
                    v => upo.dest = v },
                { "s|src=",
                    "source (to bind to) ip.ip.ip.ip:port",
                    v => upo.src = v },
                { "dll=",
                    "dll to be used to decrypt packets, must export decrypt() and init() functions.",
                    v => upo.dll = v },
                { "o|output=",
                    "where to write the output to.",
                    v => upo.output = v },
                { "m|modify",
                    "Allows decrypt() dll function to modify the data.",
                    v => upo.modify = v != null },
                {"f|format=",
                    "how to write the data: json|asciibin|dll",
                    v => upo.format = v },
                { "h|?|help",  "show this message and exit", 
                   v => show_help = v != null },
            };
            p.Parse(args);
            if (show_help)
            {
                ShowHelp(p, null);
            }

            ValidateArguments(p, upo);
            StartProxyService(upo);
        }
    }
}
