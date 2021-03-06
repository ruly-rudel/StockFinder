﻿using CsvHelper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;

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

        public IEnumerable<Stock> GetStockTable(int code, int n)
        {
            using (var db = new StockContext())
            {
                return getStockTable(db, code, n);
            }
        }

        private IEnumerable<Stock> getStockTable(StockContext db, int code, int n)
        {
            return (from r in ((from s in db.Stocks where s.Code == code orderby s.Date descending select s).Take(n)) orderby r.Date ascending select r).ToList();
        }

        public IEnumerable<StockSingleValue> GetStockMovingAverage(int code, int span, int n)
        {
            Stock[] st;
            using (var db = new StockContext())
            {
                st = (from r in (from s in db.Stocks where s.Code == code orderby s.Date descending select s).Take(n + span - 1) orderby r.Date ascending select r).ToArray();
            }

            if(st.Length >= n + span - 1)
            {
                StockSingleValue[] ssv = new StockSingleValue[n];
                for (int i = 0; i < n; i++)
                {
                    double sum = getCloseAverage(st, i, span);
                    ssv[i] = new StockSingleValue
                    {
                        Date = st[i + span - 1].Date,
                        Value = sum
                    };
                }

                return ssv;
            }
            else
            {
                return null;
            }

        }

        private double getCloseAverage(Stock[] list, int pos, int span)
        {
            double sum = 0;
            for (int j = 0; j < span; j++)
            {
                sum += list[pos + j].Close;
            }
            return sum / (double)span;
        }

        public string GetMarketStage()
        {
            int up = 0;
            int down = 0;
            int stay = 0;
            foreach (var code in GetAllStockList())
            {
                var list = GetStockTable(code, 150).ToArray();
                if (list.Length >= 150)
                {
                    double longma = getCloseAverage(list, 0, 150);
                    //double shortma = getCloseAverage(list, 100, 50);
                    Console.Out.WriteLine("code: " + code + ", MA: " + longma + ", High: " + list[149].High + ", Low: " + list[149].Low);
                    if (list[149].Low > longma && (list[149].Close - list[0].Close) > list[0].Close * 0.005)
                    {
                        up++;
                    }
                    else if (longma > list[149].High && (list[149].Close - list[0].Close) < -list[0].Close * 0.005)
                    {
                        down++;
                    }
                    else
                    {
                        stay++;
                    }
                }
            }
            return "UP : " + up.ToString() + ", Down: " + down.ToString() + ", Stay: " + stay + ".";
        }

        public IEnumerable<int> GetAllStockList()
        {
            using (var db = new StockContext())
            {
                Stock[] st = (from s in db.Stocks orderby s.Date descending select s).Take(1).ToArray();
                if(st.Length != 0)
                {
                    DateTime dt = st[0].Date;
                    return (from s in db.Stocks where s.Date == dt select s.Code).ToList();
                }
                else
                {
                    return null;
                }
            }
        }

        public IEnumerable<StockSingleValue> GetStockRelativeStrength(int code, int n)
        {
            var nikkei225 = GetStockTable(1001, n).ToArray();
            var target = GetStockTable(code, n).ToArray();

            if(nikkei225.Length == n && target.Length == n && nikkei225[n-1].Date == target[n-1].Date)
            {
                List<StockSingleValue> r = new List<StockSingleValue>();
                for (int i = 0; i < n; i++)
                {
                    r.Add(new StockSingleValue
                        {
                            Date = target[i].Date,
                            Value = target[i].Close / nikkei225[i].Close
                        }
                    );
                }

                return r;
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<StockSingleValue> GetStockSupportTrend(int code, int n)
        {
            Stock[] st;
            using (var db = new StockContext())
            {
                st = (from r in (from s in db.Stocks where s.Code == code orderby s.Date descending select s).Take(n) orderby r.Date ascending select r).ToArray();
            }

            List<StockSingleValue> ssv = new List<StockSingleValue>();
            StockSingleValue min = null;
            StockSingleValue max = null;
            int state = 0;
            if(st.Length > 0)
            {
                foreach (var s in st)
                {
                    if (min == null)
                    {
                        min = new StockSingleValue();
                        max = new StockSingleValue();
                        min.Date = s.Date;
                        min.Value = s.Low;
                        max.Date = s.Date;
                        max.Value = s.Low;
                    }
                    else
                    {
                        switch (state)
                        {
                            case 0: // downward
                                if (min.Value >= s.Low)
                                {
                                    ssv.Add(new StockSingleValue { Date = min.Date, Value = Double.NaN });
                                    min.Date = s.Date;
                                    min.Value = s.Low;
                                }
                                else
                                {
                                    ssv.Add(min);
                                    min = new StockSingleValue();
                                    max.Date = s.Date;
                                    max.Value = s.Low;
                                    state = 1;
                                }
                                break;

                            case 1: // upward
                                if (max.Value < s.Low)
                                {
                                    ssv.Add(new StockSingleValue { Date = max.Date, Value = Double.NaN });
                                    max.Date = s.Date;
                                    max.Value = s.Low;
                                }
                                else
                                {
                                    ssv.Add(new StockSingleValue { Date = max.Date, Value = Double.NaN });
                                    min.Date = s.Date;
                                    min.Value = s.Low;
                                    state = 0;
                                }
                                break;

                            default:
                                throw new NotImplementedException();

                        }
                    }
                }
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

        public void ImportZip(string n)
        {
            using (var zs = new FileStream(n, FileMode.Open))
            using (var ziparc = new ZipArchive(zs))
            {
                /*
                string sql = "Insert Into Stocks ( Code, Date, Period, \"Open\", High, Low, \"Close\", Volume ) " +
                          "Values (@Code,@Date,@Period,@Open,@High,@Low,@Close,@Volume) ";
                          */

                foreach (var i in ziparc.Entries)
                {
                    if (i.FullName.EndsWith(".zip"))
                    {
                        using (var entarc = new ZipArchive(i.Open()))
                        {
                            foreach(var j in entarc.Entries)
                            {
                                if(j.FullName.EndsWith(".txt"))
                                {
                                    using (var ent = new StreamReader(j.Open(), Encoding.GetEncoding("Shift_JIS")))
                                    {
                                        var line = ent.ReadLine().TrimEnd('\t');
                                        DateTime dt = DateTime.ParseExact(line, "yyyyMMdd", null);
                                        Console.Out.WriteLine(dt.ToShortDateString());

                                        using (var cr = new CsvReader(ent))
                                        using (var db = new StockContext())
                                        {
                                            cr.Configuration.RegisterClassMap<StockTabMap>();
                                            cr.Configuration.Delimiter = "\t";
                                            cr.Configuration.Encoding = Encoding.GetEncoding(932);
                                            cr.Configuration.HasHeaderRecord = false;
                                            while (cr.Read())
                                            {
                                                try
                                                {
                                                    var r = cr.GetRecord<StockTab>();
                                                    /*
                                                    db.Database.ExecuteSqlCommand(sql,
                                                        new SqlParameter("Code", r.Code),
                                                        new SqlParameter("Date", dt),
                                                        new SqlParameter("Period", 1),
                                                        new SqlParameter("Open", (float)r.Open),
                                                        new SqlParameter("High", (float)r.High),
                                                        new SqlParameter("Low", (float)r.Low),
                                                        new SqlParameter("Close", (float)r.Close),
                                                        new SqlParameter("Volume", (float)r.Volume)
                                                        );
                                                        */
                                                    var d = new Stock
                                                    {
                                                        Code = (int)r.Code,
                                                        Date = dt,
                                                        Period = 1,
                                                        Open = r.Open,
                                                        High = r.High,
                                                        Low = r.Low,
                                                        Close = r.Close,
                                                        Volume = r.Volume
                                                    };
                                                    db.Stocks.Add(d);
                                                } catch (CsvHelperException e)
                                                {
                                                    Console.Out.WriteLine("CVS conversion error at " + cr.Row + ": " + e.Message);
                                                }
                                            }
                                            db.SaveChanges();
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }

        }
    }

    public class Stock
    {
        //[Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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

//        public StockContext() : base("StockDatabase") { }
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

    public class StockTab
    {
        public double Code { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
    }

    public sealed class StockTabMap : CsvClassMap<StockTab>
    {
        public StockTabMap()
        {
            Map(m => m.Code).Index(0);
            Map(m => m.Open).Index(2);
            Map(m => m.High).Index(3);
            Map(m => m.Low).Index(4);
            Map(m => m.Close).Index(5);
            Map(m => m.Volume).Index(6);
        }
    }

    public class StockSingleValue
    {
        public DateTime Date { get; set; }
        public double Value { get; set; }
    }
}
