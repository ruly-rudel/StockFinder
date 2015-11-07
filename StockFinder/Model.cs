using CsvHelper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFinder.model
{
    public class ModelSingleton
    {
        private static Model _inst;
        public static Model Instance => _inst ?? (_inst = new Model());
    }

    public class Model
    {
        public List<Stock> StockTable { get; private set; } = new List<Stock>();

        public void Import(string n)
        {
            StockTable.Clear();

            using (var sr = new StreamReader(n))
            using (var cr = new CsvReader(sr))
            {
                StockTable = cr.GetRecords<Stock>().ToList();
            }

            return;
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
