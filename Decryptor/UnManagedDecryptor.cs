using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using DLLUnManager;


namespace Decryptor
{
    class UnManagedDecryptor : IDecryptor
    {
        delegate byte[] DecryptDelegate(int sender_flag, byte[] input_buffer, int buffer_size, int packet_index);
        DecryptDelegate DecryptorDecrypt;

        delegate int DecryptInitDelegate();
        DecryptInitDelegate DecryptorInit;
        
        IntPtr dll_decrypt;
        IntPtr dll_init;

        public bool LoadUnmanagedDll(string dll)
        {
            int hModule = DLLManager.LoadLibrary(dll);
            if (hModule == 0)
            {
                int err = Marshal.GetLastWin32Error();
                Console.WriteLine(err);
                Console.WriteLine("LoadLibrary Failed for {0}", dll);
                return false;
            }

            dll_init = DLLManager.GetProcAddress(hModule, "init");
            if (dll_init == IntPtr.Zero)
            {
                Console.WriteLine("Unable to find the address of init");
                return false;
            }

            DecryptorInit = (DecryptInitDelegate)Marshal.GetDelegateForFunctionPointer(dll_init, typeof(DecryptInitDelegate));
            if (DecryptorInit == null)
            {
                Console.WriteLine("decrypt's init() failed to be found!");
                return false;
            }

            dll_decrypt = DLLManager.GetProcAddress(hModule, "decrypt");
            if (dll_decrypt == IntPtr.Zero)
            {
                Console.WriteLine("Unable to find the address of decrypt!");
                return false;
            }

            DecryptorDecrypt = (DecryptDelegate)Marshal.GetDelegateForFunctionPointer(dll_decrypt, typeof(DecryptDelegate));
            if (DecryptorDecrypt == null)
            {
                Console.WriteLine("Decryptor function is null!");
                return false;
            }
            Console.WriteLine("{0} decryption dll successfully loaded!\n", dll);
            return true;
        }
        public int DecryptInit()
        {
            return DecryptorInit();
        }

        public byte[] Decrypt(int sender_flag, byte[] input_buffer, int buffer_size, int packet_index)
        {
            if (DecryptorDecrypt == null)
            {
                return null;
            }
            return DecryptorDecrypt(sender_flag, input_buffer, buffer_size, packet_index);
        }
    }
}
