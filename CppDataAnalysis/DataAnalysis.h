#pragma once

#include <iostream>
#include <vector>

//中值滤波
extern bool MeanFilter(double* source, double* destination, int length, int kernelSize);

//查找边沿点
extern bool FindEdge1(double* source, int length, double minThreshold, double maxThreshold, std::vector<int>& risingIndexs, std::vector<int>& fallingIndexs);

//查找边沿点,结果排序
extern bool FindEdge2(double* source, int length, double minThreshold, double maxThreshold, std::vector<int>& edgeIndexs, bool& isRisingFirst);

//插值检测
extern bool CheckInterpolate(std::vector<int> edgeIndexs);

//数据插值
extern bool Interpolate(std::vector<int> risingIndexs, std::vector<int> fallingIndexs, std::vector<int>& edgeIndexs, bool& isRisingFirst);

//分析脉冲
extern bool AnalysePulseData(std::vector<int> edgeIndexs, bool isRisingFirst, int sampleRate, std::vector<double>& frequency, std::vector<double>& dutyRatio);

//检测频率有效性
extern bool CheckFrequency(std::vector<double> frequency, double minFrequency, double maxFrequency, int ignoreCount);

//检测占空比有效性
extern bool CheckDutyRatio(std::vector<double> dutyRatio, double minDutyRatio, double maxDutyRatio, int ignoreCount);

//查找边沿点(对FindEdge1的标准接口封装)
extern bool FindEdge(double* source, int length, double minThreshold, double maxThreshold, int** risingIndexArray, int** fillingIndexArray, int* risingCount, int* fallingCount);

//释放指针内存资源
extern void FreeIntPtr(void* intPtr);