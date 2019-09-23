#include "DataAnalysis.h"
#include <iostream>
#include <vector>

using namespace std;

//中值滤波
//extern "C" __declspec(dllexport)
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

//extern "C" __declspec(dllexport)
bool FindEdge1(double* source, int length, double minThreshold, double maxThreshold, vector<int>& risingIndexs, vector<int>& fallingIndexs)
{
    if ((maxThreshold < minThreshold) || (length <= 0))
    {
        return false;
    }

    risingIndexs.clear();
    fallingIndexs.clear();

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
                fallingIndexs.push_back(i);
                currentLocation = -1;
            }

            break;
        case -1:
            //查找上升沿
            if (source[i] >= maxThreshold)
            {
                risingIndexs.push_back(i);
                currentLocation = 1;
            }
            break;
        default:

            //查找下降沿
            if (source[i] <= minThreshold)
            {
                currentLocation = -1;
            }

            //查找上升沿
            if (source[i] >= maxThreshold)
            {
                currentLocation = 1;
            }

            break;
        }

    }

    return true;
}

//查找边沿点,结果排序
bool FindEdge2(double* source, int length, double minThreshold, double maxThreshold, vector<int>& edgeIndexs, bool& isRisingFirst)
{
    vector<int> risingIndexs;
    vector<int> fallingIndexs;

    edgeIndexs.clear();

    bool result = FindEdge1(source, length, minThreshold, maxThreshold, risingIndexs, fallingIndexs);
    if (result == false)
    {
        return false;
    }

    return Interpolate(risingIndexs, fallingIndexs, edgeIndexs, isRisingFirst);
}

//插值检测
bool CheckInterpolate(vector<int> edgeIndexs)
{
    if (edgeIndexs.size() < 2)
    {
        return true;
    }
    for (size_t i = 0; i < edgeIndexs.size() - 1; i++)
    {
        if (edgeIndexs[i] >= edgeIndexs[i + 1])
        {
            return false;
        }
    }
    return true;
}

//数据插值
bool Interpolate(vector<int> risingIndexs, vector<int> fallingIndexs, vector<int>& edgeIndexs, bool& isRisingFirst)
{
    //上升沿点和下降沿点的
    if (std::abs((int)(risingIndexs.size() - fallingIndexs.size())) >= 2)
    {
        return false;
    }

    if ((!risingIndexs.empty()) && (!fallingIndexs.empty()))
    {
        if (risingIndexs[0] < fallingIndexs[0])
        {
            isRisingFirst = true;

            for (size_t i = 0; i < risingIndexs.size() - 1; i++)
            {
                edgeIndexs.push_back(risingIndexs[i]);
                edgeIndexs.push_back(fallingIndexs[i]);
            }
            edgeIndexs.push_back(risingIndexs[risingIndexs.size() - 1]);
            if (fallingIndexs.size() == risingIndexs.size())
            {
                edgeIndexs.push_back(fallingIndexs[risingIndexs.size() - 1]);
            }
        }
        else
        {
            isRisingFirst = false;

            for (size_t i = 0; i < fallingIndexs.size() - 1; i++)
            {
                edgeIndexs.push_back(fallingIndexs[i]);
                edgeIndexs.push_back(risingIndexs[i]);
            }
            edgeIndexs.push_back(fallingIndexs[fallingIndexs.size() - 1]);
            if (risingIndexs.size() == fallingIndexs.size())
            {
                edgeIndexs.push_back(risingIndexs[fallingIndexs.size() - 1]);
            }
        }
    }
    else if ((!risingIndexs.empty()) && fallingIndexs.empty())
    {
        if (risingIndexs.size() != 1)
        {
            return false;
        }
        isRisingFirst = true;
        edgeIndexs.push_back(risingIndexs[0]);

    }
    else if (risingIndexs.empty() && (!fallingIndexs.empty()))
    {
        if (fallingIndexs.size() != 1)
        {
            return false;
        }
        isRisingFirst = false;
        edgeIndexs.push_back(fallingIndexs[0]);
    }
    else if (risingIndexs.empty() && fallingIndexs.empty())
    {
        return false;
    }

    return CheckInterpolate(edgeIndexs);
}

//分析脉冲数据
bool AnalysePulseData(vector<int> edgeIndexs, bool isRisingFirst, int sampleRate, vector<double>& frequency, vector<double>& dutyRatio)
{
    ////一个上升沿到另一个上升沿之间的频率
    //vector<double> frequency;
    ////其中有效周期的占空比
    //vector<double> dutyRatio;

    if ((edgeIndexs.size() < 3) || (sampleRate <= 0))
    {
        return false;
    }

    if (!CheckInterpolate(edgeIndexs))
    {
        return false;
    }

    frequency.clear();
    dutyRatio.clear();

    int firstRisingIndex = isRisingFirst ? 0 : 1;
    int tempCount1 = 0;
    int tempCount2 = 0;

    //当前脉冲的频率及占空比分析
    while (firstRisingIndex + 2 < edgeIndexs.size())
    {
        tempCount1 = edgeIndexs[firstRisingIndex + 2] - edgeIndexs[firstRisingIndex];
        tempCount2 = edgeIndexs[firstRisingIndex + 1] - edgeIndexs[firstRisingIndex];
        frequency.push_back((double)sampleRate / (double)tempCount1);
        dutyRatio.push_back((double)(tempCount2)/(double)(tempCount1));
        firstRisingIndex += 2;
    }

    return true;
}

//检测频率有效性
bool CheckFrequency(vector<double> frequency, double minFrequency, double maxFrequency, int ignoreCount)
{
    int currentError = 0;

    for (auto iter = frequency.begin(); (iter != frequency.end()) && (!frequency.empty()); ++iter)
    {
        if ((*iter < minFrequency) || (*iter > maxFrequency))
        {
            currentError++;
        }
    }

    return (currentError <= ignoreCount);
}

//检测占空比有效性
bool CheckDutyRatio(vector<double> dutyRatio, double minDutyRatio, double maxDutyRatio, int ignoreCount)
{
    int currentError = 0;

    for (auto iter = dutyRatio.begin(); (iter != dutyRatio.end()) && (!dutyRatio.empty()); ++iter)
    {
        if ((*iter < minDutyRatio) || (*iter > maxDutyRatio))
        {
            currentError++;
        }
    }

    return (currentError <= ignoreCount);
}














//extern "C" __declspec(dllexport)
bool FindEdge(double* source, int length, double minThreshold, double maxThreshold, int** risingIndexArray, int** fillingIndexArray, int* risingCount, int* fallingCount)
{
    //int RisingCount = 0;
    //int FallingCount = 0;

    vector<int> risingIndexs;
    vector<int> fillingIndexs;

    if ((maxThreshold < minThreshold) || (length <= 0))
    {
        return false;
    }

    FindEdge1(source, length, minThreshold, maxThreshold, risingIndexs, fillingIndexs);

    //*risingIndexArray = (int*)malloc(risingIndexs.size() * sizeof(int));
    //*fillingIndexArray = (int*)malloc(fillingIndexs.size() * sizeof(int));
    *risingIndexArray = new int[risingIndexs.size()];
    *fillingIndexArray = new int[fillingIndexs.size()];

    int index = 0;
    for (auto iter = risingIndexs.begin(); (iter != risingIndexs.end()) && (!risingIndexs.empty()); ++iter)
    {
        (*risingIndexArray)[index++] = *iter;
    }

    index = 0;
    for (auto iter = fillingIndexs.begin(); (iter != fillingIndexs.end()) && (!fillingIndexs.empty()); ++iter)
    {
        (*fillingIndexArray)[index++] = *iter;
    }

    *risingCount = risingIndexs.size();
    *fallingCount = fillingIndexs.size();

    return true;
}

//释放指针内存资源
//extern "C" __declspec(dllexport)
void FreeIntPtr(void* intPtr)
{
    delete[] intPtr;
    //free(intPtr);
}