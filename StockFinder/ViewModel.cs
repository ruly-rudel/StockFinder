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
using Microsoft.Win32;

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
        public XyDataSeries<DateTime, double> StockGraphRS { get; } = new XyDataSeries<DateTime, double>();

        private string _StatusBarText;
        public string StatusBarText
        {
            get { return _StatusBarText; }
            private set { _StatusBarText = value; OnPropertyChanged(); }
        }

        private int _StockNum;
        public int StockNum
        {
            get { return _StockNum; }
            set { _StockNum = value; initialize(value); OnPropertyChanged(); }
        }
        public ObservableCollection<int> AllStockValue { get; set; } = new ObservableCollection<int>();

        public ICommand CmdImportCsv { get; private set; }
        public ICommand CmdImportZip { get; private set; }
        public ICommand CmdMarketTrend { get; private set; }

        // member variables
        private Model _m = ModelSingleton.Instance;

        // constructor
        public ViewModel()
        {
            CmdImportCsv = new RelayCommand(importCsv);
            CmdImportZip = new RelayCommand(importZip);
            CmdMarketTrend = new RelayCommand(getMarketTrend);
            if(_m.GetAllStockList() != null)
            {
                foreach (var i in _m.GetAllStockList())
                {
                    AllStockValue.Add(i);
                }
            }
            StockNum = 5911;
        }


        private void initialize(int code)
        {
            StockTable.Clear();
            StockGraphMA30.Clear();
            StockGraphMA10.Clear();
            StockGraphMin.Clear();
            StockGraphVolume.Clear();
            StockGraphOHLC.Clear();
            StockGraphRS.Clear();

            int n = 180;

            foreach (var i in _m.GetStockTable(code, n))
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

            var ssa = _m.GetStockMovingAverage(code, 150, n);
            if(ssa != null)
            {
                StockGraphMA30.Append(
                    from x in ssa select x.Date,
                    from x in ssa select x.Value
                );
            }

            var ssa10 = _m.GetStockMovingAverage(code, 50, n);
            if(ssa10 != null)
            {
                StockGraphMA10.Append(
                    from x in ssa10 select x.Date,
                    from x in ssa10 select x.Value
                );
            }

            var rs = _m.GetStockRelativeStrength(code, n);
            if (rs != null)
            {
                StockGraphRS.Append(
                    from x in rs select x.Date,
                    from x in rs select x.Value
                );
            }


            /*
            var min = _m.GetStockSupportTrend(num, n);
            if(min.Count() > 0)
            {
                StockGraphMin.Append(
                    from x in min select x.Date,
                    from x in min select x.Value
                );
            }
            */
        }

        // command implementation
        private void importCsv()
        {
            var dlg = new OpenFileDialog();
            dlg.DefaultExt = ".csv";
            dlg.Filter = "CSV File (.csv)|*.csv";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                _m.Import(dlg.FileName, 0);
                initialize(StockNum);
            }

        }

        private void importZip()
        {
            var dlg = new OpenFileDialog();
            dlg.DefaultExt = ".zip";
            dlg.Filter = "ZIP Archive File (.zip)|*.zip";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                _m.ImportZip(dlg.FileName);
                initialize(StockNum);
            }
        }

        private void getMarketTrend()
        {
            StatusBarText = _m.GetMarketStage();
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
