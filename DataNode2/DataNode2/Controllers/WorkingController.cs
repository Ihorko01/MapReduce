using DataNode2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataNode1.Controllers
{
    public class WorkingController : Controller
    {
        private AppDbContext db;
        private Func<string[], string> Func;
        public WorkingController(AppDbContext db)
        {
            this.db = db;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public void SetFunc(Func<string[], string> Func)
        {
            this.Func = Func;
        }
        [HttpPost]
        public void SaveFileInDB([FromBody] string name)
        {
            string a = name;
            db.Files.Add(new File { FileName = name });
            db.SaveChanges();
        }
        [HttpPost]
        public void Map([FromBody] List<string> data)
        {
            string dataReturn = string.Empty;

            var onlyText = Regex.Replace(data[0], @"\.|;|:|,|'", "");
            var words = onlyText.Split();
            foreach (var word in words)
            {
                if (word != "")
                {
                    dataReturn += word + '=' + data[1] + ';';
                }
            }
            Map map = new Map
            {
                Data = dataReturn,
                Node = ConfigurationManager.AppSettings["getPort"].ToString()
            };
            db.Maps.Add(map);
            db.SaveChanges();
            int fileId = db.Files.Max(item => item.Id);
            db.SubFiles.Add(new SubPart { FileId = fileId, MapId = map.Id, Sub = data[0] });
            db.SaveChanges();
            //var u = ConfigurationManager.AppSettings["map_return"];

            //var t = Task.Run(() => SendDataAsync(new Uri((u).ToString()), JsonConvert.SerializeObject(dataReturn)));
            //t.Wait();
        }
        [HttpGet]
        public string Shuffle()
        {
            SubPart[] subs = db.SubFiles.Where(s => s.FileId == db.Files.Max(item => item.Id)).ToArray();
            List<string> datas = new List<string>();
            foreach (var item in subs)
            {
                datas.Add(db.Maps.Where(m => m.Id == item.MapId).FirstOrDefault().Data);
            }
            string dataReturn = string.Empty;
            string dataSave = string.Empty;
            foreach (var data in datas)
            {
                string[] dataArray = data.Split(';');
                Dictionary<string, List<string>> keyListOfValue = new Dictionary<string, List<string>>();
                for (int i = 0; i < data.Split(';').Length; i++)
                {
                    if (dataArray[i] != "")
                    {
                        string key = Regex.Replace(dataArray[i].Split('=')[0], @"\.|;|:|,||/|'", "");
                        if (key != string.Empty || key != "")
                        {
                            string value = dataArray[i].Split('=')[1];

                            if (keyListOfValue.ContainsKey(key))
                            {
                                keyListOfValue[key].Add(value);
                            }
                            else
                            {
                                keyListOfValue.Add(key, new List<string> { value });
                            }
                        }
                    }
                }
                foreach (var key in keyListOfValue.Keys)
                {
                    string count = keyListOfValue[key][0];
                    for (int i = 0; i < keyListOfValue[key].Count - 1; i++)
                    {
                        count += ',' + keyListOfValue[key][i];
                    }
                    dataReturn += key + '=' + count + ';';
                    dataSave += key + '=' + count + ';';
                }
                if (dataSave != "")
                {
                    Shuffle shuffle = new Shuffle
                    {
                        Data = dataSave,
                        Node = (ConfigurationManager.AppSettings["getPort"]).ToString()
                    };
                    db.Shuffle.Add(shuffle);
                    db.SaveChanges();
                    SubPart sub = db.SubFiles.Find(db.SubFiles.Where(s => s.FileId == db.Files.Max(f => f.Id) && s.ShuffleId == null).Min(item => item.Id));
                    sub.ShuffleId = shuffle.Id;
                    db.SaveChanges();
                }
                dataSave = string.Empty;
            }
            return dataReturn;
        }
        [HttpPost]
        public void SortSave([FromBody] List<string> data)
        {
            int a = 0;
            int fileId = db.Files.Max(item => item.Id);
            db.SortDatas.Add(new SortData { Data = data[0] + '=' + data[1] + ';', FileId = fileId });
            db.SaveChanges();
        }
        [HttpPost]
        public void Reduce([FromBody] string data)
        {
            string[] keyValue = data.Split("=");

            Reduce reduce = new Reduce
            {
                Data = data,
                Node = (ConfigurationManager.AppSettings["getPort"]).ToString()
            };
            int fileId = db.Files.Max(item => item.Id);

            db.Reduce.Add(reduce);
            db.SaveChanges();
            db.SortDatas.Add(new SortData { ReduceId = reduce.Id, FileId = fileId });
            db.SaveChanges();
        }

        [HttpPost]
        public void GetReduce([FromBody] Func<string[], string> data)
        {
            int a = 0;
        }
        private string RetryReduce(Delegate method, object args)
        {
            try
            {
                string str = Convert.ToString(method.DynamicInvoke(args));
                return str;
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpGet]
        public List<string> GetSubToShuffle()
        {
            try
            {
                int fileId = db.Files.Max(f => f.Id);

                SubPart sub = db.SubFiles.Find(db.SubFiles.Where(s => s.FileId == fileId).Min(item => item.Id));
                Map map = db.Maps.Where(item => item.Id == sub.MapId).FirstOrDefault();
                Shuffle shuffle = db.Shuffle.Where(item => item.Id == sub.ShuffleId).FirstOrDefault();
                List<string> dataReturn = new List<string> { sub.Sub, map.Data, map.Node, shuffle.Data };
                db.SubFiles.Remove(sub);

                db.SaveChanges();
                db.Maps.Remove(map);
                db.Shuffle.Remove(shuffle);

                db.SaveChanges();
                return dataReturn;
            }
            catch (Exception)
            {
                return null;
            }

        }

        [HttpGet]
        public List<string> GetSortToReduce()
        {
            try
            {
                int fileId = db.Files.Max(f => f.Id);

                SortData sub = db.SortDatas.Find(db.SortDatas.Where(s => s.FileId == fileId).Min(item => item.Id));
                Reduce map = db.Reduce.Where(item => item.Id == sub.ReduceId).FirstOrDefault();
                List<string> dataReturn = new List<string> { sub.Data, map.Data, map.Node };
                db.SortDatas.Remove(sub);

                db.SaveChanges();
                db.Reduce.Remove(map);

                db.SaveChanges();
                return dataReturn;
            }
            catch (Exception)
            {
                return null;
            }

        }
    }
}
