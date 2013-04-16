using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Decryptor;

namespace UDPPDReplay
{

    class DecryptOptions
    {
        public string dll;
        public string input;
        public string output;
    }

    class DecryptFileProcessor
    {
        protected int CLIENT = 0;
        protected int SERVER = 1;
        protected DecryptOptions decrypto;
        protected IDecryptor Decrypt = null;
        
        public DecryptFileProcessor(DecryptOptions decrypto)
        {
            this.decrypto = decrypto;
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

            using (StreamReader reader = new StreamReader(decrypto.input))
            {
                StringBuilder packet = new StringBuilder();
            }
            return 0;
        }
    }
}
