using DataNode2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataNode2.Controllers
{
    namespace DataNode1.Controllers
    {
        public class WorkingController : Controller
        {
            private AppDbContext db;
            private Func<string[], string> Func;
            private int fileId;

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
                db.Files.Add(new Models.File { FileName = name });
                db.SaveChanges();
            }

            [HttpGet]
            public string[] GetFile()
            {
                return new string[] { "a", "b" };
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
                fileId = db.Files.Max(item => item.Id);
                db.SaveChanges();

                db.SubFiles.Add(new SubPart { FileId = fileId, MapId = map.Id, Sub = data[0] });
                db.SaveChanges();
                //var u = ConfigurationManager.AppSettings["map_return"];

                //var t = Task.Run(() => SendDataAsync(new Uri((u).ToString()), JsonConvert.SerializeObject(dataReturn)));
                //t.Wait();
            }

            private Dictionary<string, List<string>> CreatePairs(List<string> data, IEnumerable<string> uniqueKey)
            {
                Dictionary<string, List<string>> pairs = new Dictionary<string, List<string>>();
                foreach (var key in uniqueKey)
                {
                    foreach (var value in data)
                    {
                        if (value.Contains(key))
                        {
                            if (pairs.ContainsKey(key))
                            {
                                pairs[key].Add(value.Split('=')[1]);
                            }
                            else
                            {
                                pairs.Add(key, new List<string> { value.Split('=')[1] });
                            }
                        }
                    }
                }
                return pairs;
            }

            [HttpGet]
            public string Shuffle()
            {
                int lastFileId = db.Files.Max(item => item.Id);
                List<SubPart> subs = db.SubFiles.Where(s => s.FileId == lastFileId).ToList();
                var uniqueKey = subs.Select(s => s.Sub).Distinct();
                List<string> datas = new List<string>();
                foreach (var item in subs)
                {
                    datas.Add(db.Maps.Where(m => m.Id == item.MapId).FirstOrDefault().Data);
                }
                string dataReturn = string.Empty;
                string dataSave = string.Empty;
                var pairs = CreatePairs(datas, uniqueKey);
                foreach (var key in pairs.Keys)
                {
                    string count = pairs[key][0];
                    for (int i = 0; i < pairs[key].Count - 1; i++)
                    {
                        count += pairs[key][i];
                    }
                    dataReturn += key + '=' + count;
                    dataSave += key + '=' + count;

                    if (dataSave != "")
                    {
                        Shuffle shuffle = new Shuffle
                        {
                            Data = dataSave,
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

            [HttpGet]
            public string Exchange()
            {
                string node = ConfigurationManager.AppSettings["getPort"].ToString();
                var ports = Retrieve(new Uri((ConfigurationManager.AppSettings["get_ports"]).ToString()));
                ports.Remove(node);
                var x = db.Shuffle.Where(s => s.Node == null).ToList();
                if (x.Count != 0)
                {
                    int? id = db.Shuffle.Where(s => s.Node == null).FirstOrDefault().Id;
                    string key = db.Shuffle.Where(s => s.Node == null).FirstOrDefault().Data.Split("=")[0];
                    List<string> shuffleFromAnotherNodes = new List<string>();
                    for (int i = 0; i < ports.Count(); i++)
                    {
                        var t1 = Task.Run(() => SendKeyValue(new Uri($"{ports[i]}GetShuffleData"), key, node));
                        t1.Wait();
                        string shuffleData = t1.Result;                        //List<string> keyData = new List<string> { shuffleData.Split("=")[0] };
                        shuffleFromAnotherNodes.Add(shuffleData.Split("=")[1]);
                    }
                    string dataSave = db.Shuffle.Where(s => s.Id == id).FirstOrDefault().Data;
                    foreach (var value in shuffleFromAnotherNodes)
                    {
                        dataSave += value;
                    }
                    var result = db.Shuffle.Where(s => s.Id == id).FirstOrDefault();
                    if (result != null)
                    {
                        result.Data = dataSave;
                        result.Node = node;
                    }
                    db.SaveChanges();
                    return "True";
                }
                else
                {
                    return "False";
                }

            }

            [HttpPost]
            public string GetShuffleData([FromBody] List<string> parameters)
            {
                var record = db.Shuffle.Where(s => s.Data.Contains(parameters[0]) && s.Node == null).First();
                record.Node = parameters[1];
                db.SaveChanges();
                return record.Data;
            }

            static async Task<string> SendKeyValue(Uri u, string key, string value)
            {
                var response = string.Empty;
                using (var client = new HttpClient())
                {
                    List<string> paramList = new List<string>() { key, value };

                    string contents = JsonConvert.SerializeObject(paramList);
                    HttpRequestMessage request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = u,
                        Content = new StringContent(contents, Encoding.UTF8, "application/json")
                    };

                    HttpResponseMessage result = await client.SendAsync(request).ConfigureAwait(false);

                    response = await result.Content.ReadAsStringAsync();

                }
                return response;
            }

            [HttpPost]
            public void SortSave([FromBody] List<string> data)
            {
                int a = 0;
                int fileId = db.Files.Max(item => item.Id);
                db.SortDatas.Add(new SortData { Data = data[0] + '=' + data[1] + ';', FileId = fileId });
                db.SaveChanges();
            }

            //[HttpPost]
            //public void Reduce([FromBody] string data)
            //{
            //    string[] keyValue = data.Split("=");

            //    Reduce reduce = new Reduce
            //    {
            //        Data = data,
            //        Node = (ConfigurationManager.AppSettings["getPort"]).ToString()
            //    };
            //    int fileId = db.Files.Max(item => item.Id);

            //    db.Reduce.Add(reduce);
            //    db.SaveChanges();
            //    db.SortDatas.Add(new SortData { ReduceId = reduce.Id, FileId = fileId });
            //    db.SaveChanges();
            //}

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
            public void Clear()
            {
                var records = from m in db.Shuffle
                              select m;
                foreach (var record in records)
                {
                    db.Shuffle.Remove(record);
                }
                db.SaveChanges();
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

            static async Task<object> GetResponseFromURI(Uri u)
            {
                var response = "";
                using (var client = new HttpClient())
                {
                    HttpResponseMessage result = await client.GetAsync(u);
                    if (result.IsSuccessStatusCode)
                    {
                        response = await result.Content.ReadAsStringAsync();
                    }
                }
                return response;
            }
            private List<string> Retrieve(Uri uri)
            {
                var managePort = uri;
                var ports = (string)GetResponseFromURI(managePort).Result;
                List<string> s = new List<string>();
                foreach (var item in ports.Split(new char[] { ',', '[', ']', '\"' }))
                {
                    if (item != "")
                    {
                        s.Add(item);
                    }
                }
                return s;
            }
            public async Task<string> SendDataAsync(Uri uri, string key)
            {
                var response = string.Empty;
                using (var client = new HttpClient())
                {
                    string contents = JsonConvert.SerializeObject(key);
                    HttpRequestMessage request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = uri,
                        Content = new StringContent(contents, Encoding.UTF8, "application/json")
                    };

                    HttpResponseMessage result = await client.SendAsync(request).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        response = result.StatusCode.ToString();
                    }
                }
                return response;
            }

            [HttpGet]
            public string GetDataForReduce()
            {
                string node = ConfigurationManager.AppSettings["getPort"].ToString();
                var shuffles = db.Shuffle.Where(s => s.Node == node).ToList();
                var data = shuffles.Select(s => s.Data).ToList();
                string returnData = "";
                foreach (var item in data)
                {
                    returnData += $"{item}|";
                }
                returnData = returnData.Remove(returnData.Length - 1, 1); ;
                return returnData;
            }

            [HttpPost]
            public void Reduce([FromBody] string data)
            {
                string node = ConfigurationManager.AppSettings["getPort"].ToString();

                db.Reduce.Add(new Reduce { Data = data, Node = node });
                db.SaveChanges();

            }
        }


    }
}
