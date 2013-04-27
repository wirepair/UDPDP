using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// author @_wirepair : github.com/wirepair
// date: 04272013 
// copyright: ME AND MINE but i guess you can use it :D.
namespace Decryptor
{
    public interface IDecryptor
    {
        int Decrypt(int sender_flag, byte[] input_buffer, int buffer_size, int packet_index, ref IntPtr output, ref int output_size);

        int DecryptInit();
    }
}
