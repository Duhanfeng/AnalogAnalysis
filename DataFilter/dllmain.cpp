// dllmain.cpp : 定义 DLL 应用程序的入口点。
#include "pch.h"

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}


//中值滤波
extern "C" __declspec(dllexport)
bool MeanFilter(double* source, double* destination, int length, int kernelSize)
{
    if ((length <= 0) || (kernelSize < 3) || (length < kernelSize))
    {
        return false;
    }

    int startOffset = 0;
    int endOffset = 0;

    //偶数
    if ((kernelSize % 2) == 0)
    {
        startOffset = kernelSize / 2 - 1;
        endOffset = kernelSize / 2;
    }
    else
    {
        startOffset = endOffset = kernelSize / 2;
    }

    for (int i = 0; i < startOffset; i++)
    {
        destination[i] = source[i];
    }

    for (int i = (length - endOffset); i < length; i++)
    {
        destination[i] = source[i];
    }

    double sum = 0;

    for (int i = startOffset; i < (length - endOffset); i++)
    {
        sum = 0;
        for (int j = 0; j < kernelSize; j++)
        {
            sum += source[i - startOffset + j];
        }
        destination[i] = sum / kernelSize;
    }

    return false;
}

