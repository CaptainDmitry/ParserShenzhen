using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using NLog;
using Newtonsoft.Json;

namespace PaeserShenzhen
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static List<string> Code = new List<string>();
        public static List<string> Name = new List<string>();
        public static List<string> PreClose = new List<string>();
        public static List<string> Close = new List<string>();
        public static List<string> Change = new List<string>();
        public static List<string> Trading = new List<string>();
        public static List<string> PERatio = new List<string>();
        public static List<string> Type = new List<string>();
        [STAThread]
        static void Main(string[] args)
        {
            logger.Info("НАЧАЛО РАБОТЫ ПАРСЕРА");
            string url = "";
            DateTime date = new DateTime();
            date = DateTime.Today;            
            logger.Info("Начало загрузки 1 таблицы");
            url = "http://www.szse.cn/api/report/ShowReport/data?SHOWTYPE=JSON&CATALOGID=1394_stock&TABKEY=tab1";
            LoadStockFunds(url, date, 0);
            logger.Info("Загрузка 1 таблицы завершилась");
            logger.Info("Начало загрузки 2 таблицы");
            url = "http://www.szse.cn/api/report/ShowReport/data?SHOWTYPE=JSON&CATALOGID=1394_stock&TABKEY=tab2";
            LoadStockFunds(url, date, 1);
            logger.Info("Загрузка 2 таблицы завершилась");
            logger.Info("Начало загрузки 3 таблицы");
            url = "http://www.szse.cn/api/report/ShowReport/data?SHOWTYPE=JSON&CATALOGID=1850&TABKEY=tab1";
            LoadIndices(url, date);
            logger.Info("Загрузка 3 таблицы завершилась");
            logger.Info("Начало загрузки данных в файл");
            try
            {
                using (StreamWriter stream = new StreamWriter(args[0] + "\\" + DateTime.Today.ToString("dd MM yyyy").Replace(" ", "") + ".csv"))
                {
                    int k = 0;
                    stream.WriteLine("sep=,");
                    stream.WriteLine("Code, Name, Close, Pre-close, Change percent, Trading value, P/E Ratio, Type");
                    foreach (var item in Name)
                    {
                        stream.WriteLine(" " + Code[k] + ", " + item.Replace("&nbsp;", " ") + ", " + Close[k] + ", " + PreClose[k] + ", " +  Change[k] + ", " + Trading[k] + ", " + PERatio[k] + ", " + Type[k]);
                        k++;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Info(ex);
                logger.Debug(ex);                
            }        
            logger.Info("ОКОНЧАНИЕ РАБОТЫ ПАРСЕРА");
        }
        static void LoadStockFunds(string url, DateTime date, int table)
        {
            WebRequest webRequest;
            string ContUrl;
            bool exit = true;
            int page = 1;
            string type;
            if (table == 0)
            {
                type = "Stock";
            }
            else
            {
                type = "Fund";
            }
            while (exit)
            {
                exit = false;
                ContUrl = "&PAGENO=" + page + "&txtDate=" + date.ToString("yyyy-MM-dd");
                string json = "";             
                webRequest = (HttpWebRequest)WebRequest.Create(url + ContUrl);
                webRequest.Timeout = 400000;
                webRequest.ContentType = "/application/text";
                webRequest.Method = "GET";
                using(var response = webRequest.GetResponse())
                {
                    var result = ((HttpWebResponse)response).StatusCode;
                    using (var responseStream = response.GetResponseStream())
                    {
                        using (var streamReader = new StreamReader(responseStream))
                        {
                            json = streamReader.ReadToEnd();
                        }
                    }
                }
                try
                {              
                dynamic dataJson = JsonConvert.DeserializeObject(json);                                    
                    if (dataJson[table].data.Count != 0)
                    {
                    foreach (dynamic str in dataJson[table].data)
                        {
                            Code.Add(((string)str.zqdm).Replace(",", " "));
                            Name.Add(((string)str.zqjc).Replace(",", " "));
                            PreClose.Add(((string)str.qss).Replace(",", " "));
                            Close.Add(((string)str.ss).Replace(",", " "));
                            Change.Add(((string)str.sdf).Replace(",", " "));
                            Trading.Add(((string)str.cjje).Replace(",", " "));
                            PERatio.Add(((string)str.syl1).Replace(",", " "));
                            Type.Add(type);
                        }
                        logger.Info("Загрузка страницы " + page.ToString() + " завершена");
                        exit = true;
                        page++;
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug("Страница " + page + " не загрузилась");
                    logger.Debug(ex);
                }
            }            
        }
        static void LoadIndices(string url, DateTime date)
        {
            WebRequest webRequest;
            string ContUrl;
            bool exit = true;
            int page = 1;
            while (exit)
            {
                exit = false;              
                ContUrl = "&PAGENO=" + page + "&txtDate=" + date.ToString("yyyy-MM-dd");
                string json = "";
                webRequest = (HttpWebRequest)WebRequest.Create(url + ContUrl);
                webRequest.Timeout = 400000;
                webRequest.ContentType = "/application/text";
                webRequest.Method = "GET";
                using (var response = webRequest.GetResponse())
                {
                    var result = ((HttpWebResponse)response).StatusCode;
                    using (var responseStream = response.GetResponseStream())
                    {
                        using (var streamReader = new StreamReader(responseStream))
                        {
                            json = streamReader.ReadToEnd();
                        }
                    }
                }
                try
                {
                    dynamic dataJson = JsonConvert.DeserializeObject(json);
                    if (dataJson[0].data.Count != 0)
                    {
                        foreach (dynamic str in dataJson[0].data)
                        {
                            Code.Add(((string)str.zsdm).Replace(",", " "));
                            Name.Add(((string)str.ywmc).Replace(",", " "));
                            PreClose.Add(((string)str.qss).Replace(",", " "));
                            Close.Add(((string)str.ss).Replace(",", " "));
                            Change.Add(((string)str.sdf).Replace(",", " "));
                            Trading.Add(((string)str.cjje).Replace(",", " "));
                            PERatio.Add("");
                            Type.Add("Indice");
                        }
                        logger.Info("Загрузка страницы " + page.ToString());
                        exit = true;
                        page++;
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug("Страница " + page + " не загрузилась");
                    logger.Debug(ex);
                }
            }
        }
    }

}
