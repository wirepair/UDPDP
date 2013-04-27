using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decryptor;
using System.Runtime.InteropServices;
// author @_wirepair : github.com/wirepair
// date: 04272013 
// copyright: ME AND MINE but i guess you can use it :D.
namespace SampleManagedDecryptor
{
    public class SampleManagedDecryptor : IDecryptor
    {
        public int Decrypt(int sender_flag, byte[] input_buffer, int buffer_size, int packet_index, ref IntPtr output, ref int output_size)
        {
            output_size = buffer_size;
            output = Marshal.AllocCoTaskMem(output_size);
            
            for (int i = 0; i < output_size; i++)
            {
                input_buffer[i] ^= 0x20;
            }
            Marshal.Copy(input_buffer, 0, output, output_size);
            return 0;
        }

        public int DecryptInit()
        {
            return 0;
        }
    }
}
