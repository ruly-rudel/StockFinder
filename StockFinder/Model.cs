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
        public IEnumerable<Stock> GetStockTable()
        {
            using (var db = new StockContext())
            {
                return (from s in db.Stocks orderby s.Date ascending select s).ToList();
            }
        }

        public IEnumerable<Stock> GetStockTable(int n)
        {
            using (var db = new StockContext())
            {
                return (from r in (from s in db.Stocks orderby s.Date descending select s).Take(n) orderby r.Date ascending select r).ToList();
            }
        }

        public IEnumerable<StockSingleValue> GetStockMovingAverage(int span, int n)
        {
            Stock[] st;
            using (var db = new StockContext())
            {
                st = (from r in (from s in db.Stocks orderby s.Date descending select s).Take(n + span - 1) orderby r.Date ascending select r).ToArray();
            }

            StockSingleValue[] ssv = new StockSingleValue[n];
            for(int i = 0; i < n; i++)
            {
                double sum = 0;
                for(int j = 0; j < span; j++)
                {
                    sum += st[i + j].Close;
                }
                ssv[i] = new StockSingleValue
                {
                    Date = st[i + span - 1].Date,
                    Value = sum / span
                };
            }

            return ssv;
        }


        public void Import(string n, int code)
        {
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

    public class StockSingleValue
    {
        public DateTime Date { get; set; }
        public double Value { get; set; }
    }
}
