using Abt.Controls.SciChart.Model.DataSeries;
using CsvHelper;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace StockFinder.viewmodel
{
    public class ViewModel
    {
        public ObservableCollection<Stock> StockTable { get; private set; }
        public OhlcDataSeries<DateTime, double> StockGraphOHLC { get; private set; }
        public XyDataSeries<DateTime, double> StockGraphMA { get; private set; }

        private CommandImport _commandImport;
        public ICommand CmdImport { get { return _commandImport ?? (_commandImport = new CommandImport(this)); } }

        public ViewModel()
        {
            StockTable = new ObservableCollection<Stock>();
            StockGraphOHLC = new OhlcDataSeries<DateTime, double>();
            StockGraphMA = new XyDataSeries<DateTime, double>();
        }

    }

    public class CommandImport : ICommand
    {
        private ViewModel _vm;

        public CommandImport(ViewModel vm)
        {
            _vm = vm;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            using (var sr = new StreamReader("stock_data_week.csv"))
            {
                using (var cr = new CsvReader(sr))
                {
                    while (cr.Read())
                    {
                        var r = cr.GetRecord<Stock>();
                        _vm.StockTable.Add(r);
                    }
                }
            }
            _vm.StockGraphOHLC.Append(
                from x in _vm.StockTable select x.Date,
                from x in _vm.StockTable select x.Open,
                from x in _vm.StockTable select x.High,
                from x in _vm.StockTable select x.Low,
                from x in _vm.StockTable select x.Close
            );
            _vm.StockGraphMA.Append(
                from x in _vm.StockTable select x.Date,
                from x in _vm.StockTable select x.Close
            );
        }
    }

    public class Stock
    {
        public DateTime Date { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
    }
}
