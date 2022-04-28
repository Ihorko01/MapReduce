using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using System.Net;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace Library
{
    public class Methods
    {
        public Uri ManagePort = new Uri("https://localhost:44376/Manage/GetPorts");
        public Uri ManageSaveInfoMap = new Uri("https://localhost:44376/Manage/SaveInfoMap");
        public Uri ManageSaveInfoReduce = new Uri("https://localhost:44376/Manage/SaveInfoReduce");
        public Uri ManageSaveInfoShuffle = new Uri("https://localhost:44376/Manage/SaveInfoShuffle");
        public Uri ManageShow = new Uri("https://localhost:44376/Manage/Show");
        public Uri ManageClearDB = new Uri("https://localhost:44376/Manage/ClearDataBases");


        public Func<Dictionary<string, string>, Dictionary<string, string>> FuncBegin { get; set; }
        public Func<string[], string> FuncResult { get; set; }

        public class FileUpload
        {
            public string FilePath { get; set; }
        }

        private CancellationTokenSource cts = new CancellationTokenSource();

        public void Run(string path, int columnKey, int columnValue)
        {
            ClearDbAsync(ManageClearDB);
            BeginMap(path, columnKey, columnValue);
            var dic = BeginShuffle();
            BeginReduce(dic);
            ShowResult();
        }

        private async void ShowResult()
        {
            var ports = Retrieve(ManagePort);
            var p1 = ports;
            var p2 = ports;
            int len = ports.Count;
            int i = 0;
            List<List<string>> data = new List<List<string>>();
            while (p1.Count != 0)
            {
                List<string> firstPart = Retrieve(new Uri(p1[i % p1.Count] + "GetSubToShuffle"));
                if (firstPart.Count == 0)
                {
                    p1.Remove(p1[i % p1.Count]);
                }
                else
                {
                    data.Add(firstPart);
                }
                i++;
            }
            while (p2.Count != 0)
            {
                List<string> secondPart = Retrieve(new Uri(p2[i]));
                if (secondPart == null)
                {
                    p2.Remove(p2[i]);
                }
                else
                {
                    data.Add(secondPart);
                }
                i++;
            }
            SendDataAsync(ManageShow, data);
        }

        public async Task SendDataAsync(Uri uri, List<List<string>> content)
        {
            var response = string.Empty;
            using (var client = new HttpClient())
            {
                List<List<string>> paramList = content;

                string contents = JsonConvert.SerializeObject(paramList);
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
        }

        private async void BeginReduce(Dictionary<string, List<string>> dic)
        {
            var ports = Retrieve(ManagePort);
            int i = 0;

            foreach (var key in dic.Keys)
            {
                string count = string.Empty;

                count = RetryReduce(FuncResult, dic[key].ToArray());
                string content = $"{key}={count};";
                Stopwatch sw = Stopwatch.StartNew();
                var t1 = Task.Run(() => SendDataAsync(new Uri((ports[i % ports.Count] + "Reduce").ToString()), content));
                Console.WriteLine($"Reduce-{ports[i % ports.Count]}\t{t1.Result}");
                t1.Wait();
                sw.Stop();
                if (t1.Result.ToString() == "OK")
                {
                    var t2 = Task.Run(() => SendDataSave(ManageSaveInfoReduce, new List<object> { ports[i % ports.Count], sw.Elapsed }).ToString());
                    t2.Wait();
                }
                i++;
            }
        }

        private Dictionary<string, List<string>> BeginShuffle()
        {
            var ports = Retrieve(ManagePort);
            List<string> data = new List<string>();
            for (int i = 0; i < ports.Count; i++)
            {
                Stopwatch sw = Stopwatch.StartNew();
                data.Add(GetResponseFromURI(new Uri(ports[i] + "Shuffle")).Result);
                Console.WriteLine($"Shuffle-{ports[i]}");
                sw.Stop();
                var t2 = Task.Run(() => SendDataSave(ManageSaveInfoShuffle, new List<object> { ports[i % ports.Count], sw.Elapsed }).ToString());
                t2.Wait();
            }
            return SortData(data);
        }

        private Dictionary<string, List<string>> SortData(List<string> shuffle)
        {
            var ports = Retrieve(ManagePort);
            Dictionary<string, List<string>> sortData = new Dictionary<string, List<string>>();
            for (int i = 0; i < shuffle.Count; i++)
            {
                string[] data = shuffle[i].Split(';');
                for (int j = 0; j < data.Length; j++)
                {
                    string[] keyValue = data[j].Split('=');
                    if (keyValue.Length > 1)
                    {
                        if (sortData.ContainsKey(keyValue[0]))
                        {
                            sortData[keyValue[0]].Add(keyValue[1]);
                        }
                        else
                        {
                            List<string> values = new List<string>();
                            for (int u = 0; u < keyValue[1].Split(',').Length; u++)
                            {
                                values.Add(keyValue[1]);
                            }
                            sortData.Add(keyValue[0], values);
                        }
                    }
                }
            }
            var p = sortData.OrderBy(k => k.Key);
            var dic = p.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
            string dataReturn = string.Empty;

            return dic;
        }

        [HttpPost]
        private async void BeginMap(string path, int key, int value)
        {
            if (File.Exists(path))
            {
                var ports = Retrieve(ManagePort);
                string filename = Path.GetFileName(path);
                List<List<string>> Blocks = new List<List<string>>();
                List<string> list = File.ReadAllLines(path).ToList();
                for (int i = 0; i < ports.Count; i++)
                {
                    string content = filename;
                    var t = Task.Run(() => SendDataAsync(new Uri((ports[i] + "SaveFileInDB").ToString()), content));
                    t.Wait();
                }
                int p = 0;
                string[] headers = list[0].Split(',');
                Dictionary<string, string> headValuePairs = new Dictionary<string, string>();
                for (int i = 1; i < list.Count; i++)
                {
                    string lineKey = string.Empty;
                    string lineValue = string.Empty;
                    string[] split = list[i].Split(',');

                    for (int y = 0; y < headers.Length; y++)
                    {
                        headValuePairs.Add(headers[y], split[y]);
                    }
                    lineKey += split[key];
                    lineValue += split[value];
                    Blocks.Add(new List<string> { lineKey, lineValue });
                    Dictionary<string, string> pairs = RetryMap(FuncBegin, Blocks[i][0]) as Dictionary<string, string>;

                    string content = new Uri(ports[p % ports.Count]).LocalPath.ToString();
                    Stopwatch sw = Stopwatch.StartNew();
                    var t1 = Task.Run(() => SendKeyValue(new Uri((ports[p % ports.Count] + "Map").ToString()), pairs, Blocks[i][1]));
                    Console.WriteLine($"Map-{ports[p % ports.Count]}\t{t1.Result}");
                    t1.Wait();
                    sw.Stop();
                    if (t1.Result.ToString() == "OK")
                    {
                        var t2 = Task.Run(() => SendDataSave(ManageSaveInfoMap, new List<object> { ports[p % ports.Count], sw.Elapsed, Blocks[i][0], Blocks[i][1] }).ToString());
                        t2.Wait();
                    }
                    p++;

                }
            }
        }

        private Dictionary<string, string> InitialFileProcessing(Delegate method, object parameters)
        {
            Dictionary<string, string> convertData = new Dictionary<string, string>();
            return convertData;
        }

        private object RetryMap(Delegate method, object row)
        {
            return method.DynamicInvoke(row);
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
                if (result.IsSuccessStatusCode)
                {
                    response = result.StatusCode.ToString();
                }
            }
            return response;
        }

        public async Task<string> SendDataAsync(Uri uri, string data)
        {
            var response = string.Empty;
            using (var client = new HttpClient())
            {
                string contents = JsonConvert.SerializeObject(data);
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

        public async Task<string> SendDataSave(Uri uri, List<object> data)
        {
            var response = string.Empty;
            using (var client = new HttpClient())
            {
                string contents = JsonConvert.SerializeObject(data);
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

        //static async Task<string> SendURI(Uri u, string content)
        //{
        //    HttpContent c = new StringContent(content, Encoding.UTF8, "application/json");

        //    var response = string.Empty;
        //    using (var client = new HttpClient())
        //    {
        //        HttpRequestMessage request = new HttpRequestMessage
        //        {
        //            Method = HttpMethod.Post,
        //            RequestUri = u,
        //            Content = c
        //        };

        //        HttpResponseMessage result = await client.SendAsync(request);
        //        if (result.IsSuccessStatusCode)
        //        {
        //            response = result.StatusCode.ToString();
        //        }
        //    }
        //    return response;
        //}

        static async Task<string> GetResponseFromURI(Uri u)
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

        private async Task ClearDbAsync(Uri uri)
        {
            var managePort = uri;
            using (var client = new HttpClient())
            {

                var response = await client.PostAsync(managePort, null);

                var responseString = await response.Content.ReadAsStringAsync();
            }
        }
        //static async Task<string> PostURI(Uri u)
        //{
        //    var response = string.Empty;
        //    using (var client = new HttpClient())
        //    {
        //        HttpRequestMessage request = new HttpRequestMessage
        //        {
        //            Method = HttpMethod.Post,
        //            RequestUri = u
        //        };

        //        HttpResponseMessage result = await client.SendAsync(request);
        //        if (result.IsSuccessStatusCode)
        //        {
        //            response = result.StatusCode.ToString();
        //        }
        //    }
        //    return response;
        //}


        private List<string> Retrieve(Uri uri)
        {
            var managePort = uri;
            var ports = GetResponseFromURI(managePort).Result;
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
    }
}
