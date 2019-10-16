
// Device's information structure, macros and interface function definition.
#ifndef _DEVICE_HEADER
#define _DEVICE_HEADER

// Windows Header Files:
#include <windows.h>
#include <winioctl.h>
#include <stdio.h>
#include <stdlib.h>
#include <stdarg.h>
#include <malloc.h>



//////////////////////////////////////////////////////////////////////////////////
UCHAR WINAPI USBCtrlTrans( UCHAR Request, USHORT Value, DWORD outBufSize);
ULONG WINAPI DeviceOpen();
ULONG WINAPI DeviceClose();
ULONG WINAPI AiReadBulkData ( ULONG SampleCount, unsigned int eventNum,ULONG TimeOut, char * Buffer );  
ULONG WINAPI EventCheck( LONG Timeout );
ULONG WINAPI CLearBuffer(char obj,unsigned int Num);
void SpecifyDevIdx(int idx);
DWORD WINAPI USBCtrlTransSimple( DWORD Request);
PUCHAR WINAPI  GetBuffer4Wr(int index);
VOID WINAPI  SetInfo(double dataNumPerPixar,double CurrentFreq, unsigned char ChannelMask, int ZrroUniInt,unsigned int  bufferoffset, unsigned int  HWbufferSize);
///////////////////////////////////////////////////////////////////////////////////
