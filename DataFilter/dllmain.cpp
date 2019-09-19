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

    return true;
}

extern "C" __declspec(dllexport)
bool FindEdge(double* source, int length, double minThreshold, double maxThreshold,  int* topLocation, int* buttomLocation, int *risingCount, int *fallingCount)
{
    int RisingCount = 0;
    int FallingCount = 0;

    if ((maxThreshold < minThreshold) || (length <= 0))
    {
        return false;
    }

    //当前位置,0-最大阈值与最小阈值之间,-1小于最小阈值,1-大于最大阈值
    int currentLocation = 0;

    if (source[0] >= maxThreshold)
    {
        currentLocation = 1;
    }
    else if (source[0] <= minThreshold)
    {
        currentLocation = -1;
    }
    else
    {
        currentLocation = 0;
    }

    for (int i = 1; i < length; i++)
    {
        switch (currentLocation)
        {
        case 1:
            //查找下降沿
            if (source[i] <= minThreshold)
            {
                buttomLocation[FallingCount] = i;
                ++FallingCount;
                currentLocation = -1;
            }

            break;
        case -1:
            //查找上升沿
            if (source[i] >= maxThreshold)
            {
                topLocation[RisingCount] = i;
                ++RisingCount;
                currentLocation = 1;
            }
            break;
        default:

            //查找下降沿
            if (source[i] <= minThreshold)
            {
                buttomLocation[FallingCount] = i;
                ++FallingCount;
                currentLocation = -1;
            }

            //查找上升沿
            if (source[i] >= maxThreshold)
            {
                topLocation[RisingCount] = i;
                ++RisingCount;
                currentLocation = 1;
            }

            break;
        }

        *risingCount = RisingCount;
        *fallingCount = FallingCount;
    }

    return true;
}