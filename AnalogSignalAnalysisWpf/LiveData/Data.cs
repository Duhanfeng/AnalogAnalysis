using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf.LiveData
{
    public class Data
    {
        public Data()
        {

        }
        public Data(double value, double value1)
        {
            //Date = date;
            Value = value;
            Value1 = value1;
            //Value2 = value2;
        }

        //public DateTime Date
        //{
        //    get;
        //    set;
        //}

        public double Value
        {
            get;
            set;
        }
        public double Value1
        {
            get;
            set;
        }
        //public double Value2
        //{
        //    get;
        //    set;
        //}
    }
}
