using Abt.Controls.SciChart.Model.DataSeries;
using CsvHelper;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace StockFinder.viewmodel
{
    public class ViewModel
    {
        public ObservableCollection<Stock> StockTable { get; private set; }
        public OhlcDataSeries<DateTime, double> StockGraphOHLC { get; private set; }
        public XyDataSeries<DateTime, double> StockGraphMA { get; private set; }

        public ViewModel()
        {
            StockTable = new ObservableCollection<Stock>();
            StockGraphOHLC = new OhlcDataSeries<DateTime, double>();
            StockGraphMA = new XyDataSeries<DateTime, double>();

            using (var sr = new StreamReader("stock_data_week.csv"))
            {
                using (var cr = new CsvReader(sr))
                {
                    while (cr.Read())
                    {
                        var r = cr.GetRecord<Stock>();
                        StockTable.Add(r);
                    }
                }
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

        public class Stock
        {
            public DateTime Date { get; set; }
            public double Open { get; set; }
            public double High { get; set; }
            public double Low { get; set; }
            public double Close { get; set; }
        }
    }
}
