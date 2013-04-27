using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Options;
using UDPDPLogWriter;
// author @_wirepair : github.com/wirepair
// date: 04272013 
// copyright: ME AND MINE but i guess you can use it :D.
namespace UDPPDReplay
{
    class Program
    {
        static void ShowHelp(OptionSet p, string error)
        {
            if (error != null)
            {
                Console.WriteLine("Error: {0}",error);
                Console.WriteLine();
            }
            Console.WriteLine("Usage: UDPDPReplay [OPTIONS]");
            Console.WriteLine("decrypt traffic of proxied data, after the fact.");
            Console.WriteLine("Possible commands:");
            Console.WriteLine();
            p.WriteOptionDescriptions(Console.Out);
            Console.WriteLine("Example:\nUDPDPReplay -i <cap.log> -f <format> -o <outfile> --of=asciinbin --dll=C:\\path\to\\Decryptor.dll");
            Environment.Exit(-1);
        }

        static void Main(string[] args)
        {
            bool show_help = false;
            DecryptFileProcessor dfp = null;
            DecryptOptions decrypto = new DecryptOptions();
            var p = new OptionSet() 
            {
                { "d|dll=",
                    "dll to be used to decrypt packets, must export decrypt() and init() functions.",
                    v => decrypto.dll = v },
                { "i|input=",
                    "logfile where raw data was captured.",
                    v => decrypto.input = v },
                { "f|format=",
                    "logfile format for how the data was captured.",
                    v => decrypto.format = v },
                { "o|output=",
                    "Where to store the decrypted output.",
                    v => decrypto.output = v },
                {"of|outputformat=",
                    "Output format (json|asciibin).",
                    v => decrypto.outputformat = v},
                { "h|?|help",  "show this message and exit", 
                   v => show_help = v != null },
            };
            p.Parse(args);
            if (show_help)
            {
                ShowHelp(p, null);
            }
            
            if (decrypto.input == null)
            {
                ShowHelp(p, "input is required.");
            }
            else if (decrypto.output == null)
            {
                ShowHelp(p, "output is required.");
            }
            else if (decrypto.format == null)
            {
                ShowHelp(p, "format is required.");
            }

            if (decrypto.dll != null)
            {
                dfp = new DecryptFileProcessor(decrypto);
                dfp.ProcessData();
            }
            else
            {
                IULogWriter logger = null;
                if (decrypto.format.Equals("json"))
                {
                    logger = new JSONLogWriter();
                    logger.Open(decrypto.input, "r");
                    Transmissions trans = logger.ReadTransmission();
                    foreach (LogEvent e in trans.LogEventList)
                    {
                        Console.WriteLine("{0} {1} {2}", e.sender, e.count, Encoding.ASCII.GetString(e.data));
                    }
                }
            }
        }
    }
}
