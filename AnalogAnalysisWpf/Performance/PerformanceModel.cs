using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogAnalysisWpf.Performance
{
    public class PerformanceModel
    {
        public PerformanceModel(DateTime date, double value)
        {
            Date = date;
            Value = value;
        }

        public DateTime Date
        {
            get;
            set;
        }

        public double Value
        {
            get;
            set;
        }
    }
}
