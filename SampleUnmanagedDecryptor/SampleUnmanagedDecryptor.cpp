// SampleUnmanagedDecryptor.cpp : Opens a file for logging in init, xors all input data with 0x20;
// author @_wirepair : github.com/wirepair
// date: 04272013 
// copyright: ME AND MINE but i guess you can use it :D.

#include "stdafx.h"

FILE *outfile;

int init()
{
	int err = fopen_s(&outfile, "zoop.txt", "w");
	return err;
}

int decrypt(int sender_flag, char *input_buffer, unsigned int size, int packet_index, unsigned char** output_buffer, unsigned int* output_size)
{

	if (input_buffer == NULL)
	{
		return -1;
	}
	*output_size = size;
	fprintf(outfile, "output_size: %d input_buffer: %s\n", *output_size, input_buffer);
	fflush(outfile);
	*output_buffer = (unsigned char *)LocalAlloc(LMEM_ZEROINIT, size*sizeof(unsigned char));
	fprintf(outfile, "output buffer: %p\n", output_buffer);
	fflush(outfile);
	memcpy(*output_buffer, input_buffer, *output_size);
	
	for (unsigned int i = 0; i < size; i++)
	{
		fprintf(outfile, "%c", (*output_buffer)[i]);
		fflush(outfile);
		(*output_buffer)[i] ^= 0x20;
	}
	return 0;
}