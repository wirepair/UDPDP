using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Decryptor;

namespace SampleDecryptor
{
    class SampleManagedDecryptor : IDecryptor
    {
        public byte[] Decrypt(int sender_flag, byte[] input_buffer, int buffer_size, int packet_index)
        {
            for (int i = 0; i < buffer_size; i++)
            {
                input_buffer[i] ^= 0x20;
            }
            return input_buffer;
        }

        public int DecryptInit()
        {
            return 0;
        }
    }
}
