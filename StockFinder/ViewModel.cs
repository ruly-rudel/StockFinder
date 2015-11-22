using Abt.Controls.SciChart.Model.DataSeries;
using CsvHelper;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using StockFinder.util;
using StockFinder.model;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StockFinder.viewmodel
{
    public class ViewModel : INotifyPropertyChanged
    {
        // property
        public ObservableCollection<Stock> StockTable { get; } = new ObservableCollection<Stock>();
        public OhlcDataSeries<DateTime, double> StockGraphOHLC { get; } = new OhlcDataSeries<DateTime, double>();
        public XyDataSeries<DateTime, double> StockGraphVolume { get; } = new XyDataSeries<DateTime, double>();
        public XyDataSeries<DateTime, double> StockGraphMA30 { get; } = new XyDataSeries<DateTime, double>();
        public XyDataSeries<DateTime, double> StockGraphMA10 { get; } = new XyDataSeries<DateTime, double>();
        public XyDataSeries<DateTime, double> StockGraphMin { get; } = new XyDataSeries<DateTime, double>();

        private string _StatusBarText;
        public string StatusBarText
        {
            get { return _StatusBarText; }
            private set { _StatusBarText = value; OnPropertyChanged(); }
        }

        public ICommand CmdImportCsv { get; private set; }
        public ICommand CmdImportZip { get; private set; }

        // member variables
        private Model _m = ModelSingleton.Instance;

        // constructor
        public ViewModel()
        {
            CmdImportCsv = new RelayCommand(importCsv);
            CmdImportZip = new RelayCommand(importZip);
            initialize();
            StatusBarText = "Ok.";
        }


        private void initialize()
        {
            StockTable.Clear();
            StockGraphMA30.Clear();
            StockGraphMA10.Clear();
            StockGraphMin.Clear();
            StockGraphVolume.Clear();
            StockGraphOHLC.Clear();

            int n = 200;

            foreach (var i in _m.GetStockTable(0, n))
            {
                StockTable.Add(i);
            }
            StockGraphOHLC.Append(
                from x in StockTable select x.Date,
                from x in StockTable select x.Open,
                from x in StockTable select x.High,
                from x in StockTable select x.Low,
                from x in StockTable select x.Close
            );

            StockGraphVolume.Append(
                from x in StockTable select x.Date,
                from x in StockTable select x.Volume
            );

            var ssa = _m.GetStockMovingAverage(0, 30, n);
            if(ssa != null)
            {
                StockGraphMA30.Append(
                    from x in ssa select x.Date,
                    from x in ssa select x.Value
                );
            }

            var ssa10 = _m.GetStockMovingAverage(0, 10, n);
            if(ssa10 != null)
            {
                StockGraphMA10.Append(
                    from x in ssa10 select x.Date,
                    from x in ssa10 select x.Value
                );
            }

            var min = _m.GetStockSupportTrend(0, n);
            if(min.Count() > 0)
            {
                StockGraphMin.Append(
                    from x in min select x.Date,
                    from x in min select x.Value
                );
            }
        }

        // command implementation
        private void importCsv()
        {
            _m.Import("stock_data_week.csv", 0);

            initialize();
        }

        private void importZip()
        {
            foreach(var i in _m.ImportZip("2014.zip"))
            {
                StatusBarText = i;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
