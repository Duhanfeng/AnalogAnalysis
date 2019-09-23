
#include <iostream>
#include <string>
#include <vector>
#include "csv.h"
#include "DataAnalysis.h"

using namespace std;

int main()
{
    bool result = false;
    vector<double> sourceVector;
    double* source = nullptr;

    io::CSVReader<1>in("data/高频1.csv");

    in.read_header(io::ignore_missing_column | io::ignore_extra_column, "ch1");

    double data1;
    while (in.read_row(data1))
    {
        sourceVector.push_back(data1);
    }

    if (!sourceVector.empty())
    {
        //向量转数组
        source = new double[sourceVector.size()];
        int index = 0;
        for (auto iter = sourceVector.begin(); (iter != sourceVector.end()) && (!sourceVector.empty()); ++iter)
        {
            source[index++] = *iter;
        }

        vector<int> risingIndexs;
        vector<int> fallingIndexs;
        vector<int> edgeIndexs;
        bool isFillingFirst = true;

        //查找边沿
        FindEdge1(source, sourceVector.size(), 1.5, 3, risingIndexs, fallingIndexs);
        Interpolate(risingIndexs, fallingIndexs, edgeIndexs, isFillingFirst);

        bool isFilling = isFillingFirst;
        string edge;
        for (size_t i = 0; i < edgeIndexs.size(); i++)
        {
            edge = isFilling ? "↑" : "↓";
            std::cout << edge << edgeIndexs[i] << std::endl;
            isFilling = !isFilling;
        }
        std::cout << std::endl << std::endl << std::endl;

        vector<double> frequency;
        vector<double> dutyRatio;

        //数据分析
        AnalysePulseData(edgeIndexs, isFillingFirst, 200*1000, frequency, dutyRatio);
        for (size_t i = 0; i < frequency.size(); i++)
        {
            cout << frequency[i] << "(Hz)  " << dutyRatio[i] << "%" << endl;
        }
        result = CheckFrequency(frequency, 4.5, 7.5, 0);

        cout << "result: " << (result ? "OK" : "NG") << endl;
    }
    
    //释放资源
    delete[] source;
    source = nullptr;

    while (!getchar());

}

