using Abt.Controls.SciChart.Model.DataSeries;
using CsvHelper;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using StockFinder.util;
using StockFinder.model;

namespace StockFinder.viewmodel
{
    public class ViewModel
    {
        // property
        public ObservableCollection<Stock> StockTable { get; } = new ObservableCollection<Stock>();
        public OhlcDataSeries<DateTime, double> StockGraphOHLC { get; } = new OhlcDataSeries<DateTime, double>();
        public XyDataSeries<DateTime, double> StockGraphMA { get; } = new XyDataSeries<DateTime, double>();

        public ICommand CmdImport { get; private set; }

        // member variables
        private Model _m = ModelSingleton.Instance;

        // constructor
        public ViewModel()
        {
            CmdImport = new RelayCommand(import);
            initialize();
        }


        private void initialize()
        {
            StockTable.Clear();
            StockGraphMA.Clear();
            StockGraphOHLC.Clear();

            foreach (var i in _m.StockTable)
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
            StockGraphMA.Append(
                from x in StockTable select x.Date,
                from x in StockTable select x.Close
            );
        }

        // command implementation
        private void import()
        {
            _m.Import("stock_data_week.csv", 0);

            initialize();
        }
    }
}
