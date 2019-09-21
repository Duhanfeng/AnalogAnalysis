using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogDataAnalysisWpf.LiveData
{
    public class LiveDataViewModel : Screen
    {
        public LiveDataViewModel()
        {
            Collection = new ObservableCollection<Data>();
        }


        private ObservableCollection<Data> collection;
        public ObservableCollection<Data> Collection
        {
            get
            {
                return collection;
            }
            set
            {
                collection = value;
                NotifyOfPropertyChange(() => Collection);
            }
        }

        private ObservableCollection<Data> collection2;
        public ObservableCollection<Data> Collection2
        {
            get
            {
                return collection2;
            }
            set
            {
                collection2 = value;
                NotifyOfPropertyChange(() => Collection2);
            }
        }
    }
}
