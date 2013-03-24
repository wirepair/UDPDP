using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decryptor
{
    public interface IDecryptor
    {
        byte[] Decrypt(int sender_flag, byte[] input_buffer, int buffer_size, int packet_index);
        int DecryptInit();
    }
}
