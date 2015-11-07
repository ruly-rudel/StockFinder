using CsvHelper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace StockFinder.model
{
    public class ModelSingleton
    {
        private static Model _inst;
        public static Model Instance => _inst ?? (_inst = new Model());
    }

    public class Model
    {
        public List<Stock> StockTable { get; private set; }

        public Model()
        {
            using (var db = new StockContext())
            {
                StockTable = (from s in db.Stocks select s).ToList();
            }
        }

        public void Import(string n, int code)
        {
            StockTable.Clear();

            using (var db = new StockContext())
            using (var sr = new StreamReader(n))
            using (var cr = new CsvReader(sr))
            {
                while(cr.Read())
                {
                    var r = cr.GetRecord<StockCsv>();
                    var d = new Stock
                    {
                        Code = code,
                        Date = r.Date,
                        Period = 2,
                        Open = r.Open,
                        High = r.High,
                        Low = r.Low,
                        Close = r.Close,
                        Volume = r.Volume
                    };
                    db.Stocks.Add(d);
                }
                db.SaveChanges();

                StockTable = (from s in db.Stocks select s).ToList();
            }


            return;
        }
    }

    public class Stock
    {
        [Key]
        public int Id { get; set; }
        public int Code { get; set; }
        public DateTime Date { get; set; }
        public int Period { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
    }

    public class StockContext : DbContext
    {
        public DbSet<Stock> Stocks { get; set; }

        public StockContext() : base("StockDatabase") { }
    }


    public class StockCsv
    {
        public DateTime Date { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
    }
}
