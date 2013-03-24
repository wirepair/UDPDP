using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Options;

namespace UDPPDReplay
{
    class Program
    {
        class DecryptOptions
        {
            public string dll;
            public string input;
            public string output;
        }

        static void ShowHelp(OptionSet p, string error)
        {
            if (error != null)
            {
                Console.WriteLine(error);
                Console.WriteLine();
            }
            Console.WriteLine("Usage: UDPPDReplay [OPTIONS]");
            Console.WriteLine("Used decrypt traffic of proxied data, after the fact.");
            Console.WriteLine("Possible commands:");
            Console.WriteLine();
            p.WriteOptionDescriptions(Console.Out);
            Console.WriteLine("Example:\nUDPPDReplay -i <cap.log> -o <outfile> --dll=Decryptor.dll");
            Environment.Exit(-1);
        }

        static void Main(string[] args)
        {
            bool show_help = false;
            DecryptOptions decrypto = new DecryptOptions();
            var p = new OptionSet() 
            {
                { "dll=",
                    "dll to be used to decrypt packets, must export decrypt() and init() functions.",
                    v => decrypto.dll = v },
                { "i|input=",
                    "logfile where raw data was captured.",
                    v => decrypto.input = v },
                { "o|output",
                    "Where to store the decrypted output.",
                    v => decrypto.output = v },
                { "h|?|help",  "show this message and exit", 
                   v => show_help = v != null },
            };
            p.Parse(args);
            if (show_help)
            {
                ShowHelp(p, null);
            }
        }
    }
}
