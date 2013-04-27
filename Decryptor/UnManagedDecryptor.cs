using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using DLLUnManager;
using System.ComponentModel;

// author @_wirepair : github.com/wirepair
// date: 04272013 
// copyright: ME AND MINE but i guess you can use it :D.
namespace Decryptor
{
    class UnManagedDecryptor : IDecryptor
    {
        delegate int DecryptDelegate(int sender_flag, byte[] input_buffer, int buffer_size, int packet_index, ref IntPtr output, ref int output_size);
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
                Console.WriteLine("LoadLibrary Failed for {0} code ({1}): {2}", dll, err, new Win32Exception(Marshal.GetLastWin32Error()).Message);
                return false;
            }
            Console.WriteLine("LoadLibrary Success.");
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
            Console.WriteLine("GetProcAddress of init() success.");
            dll_decrypt = DLLManager.GetProcAddress(hModule, "decrypt");
            if (dll_decrypt == IntPtr.Zero)
            {
                Console.WriteLine("Unable to find the address of decrypt!");
                return false;
            }
            Console.WriteLine("GetProcAddress of decrypt() success.");
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

        public int Decrypt(int sender_flag, byte[] input_buffer, int buffer_size, int packet_index, ref IntPtr output, ref int output_size)
        {
            if (DecryptorDecrypt == null)
            {
                return -1;
            }
            return DecryptorDecrypt(sender_flag, input_buffer, buffer_size, packet_index, ref output, ref output_size);
        }
    }
}
