// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

#include "targetver.h"

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files:
#include <windows.h>
#include <stdio.h>
#include <stdlib.h>

// externs
extern "C" __declspec(dllexport) int decrypt(int sender_flag, char *input_buffer, unsigned int size, int packet_index, unsigned char** output_buffer, unsigned int* output_size);
extern "C" __declspec(dllexport) int init(void);


// TODO: reference additional headers your program requires here
