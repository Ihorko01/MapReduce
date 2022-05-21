using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
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
        public Uri ManageSaveFilePath = new Uri("https://localhost:44376/Manage/SaveInfoFile");
        public Uri ManageSaveInfoLine = new Uri("https://localhost:44376/Manage/SaveInfoLine");
        public Uri ManageSaveInfoMap = new Uri("https://localhost:44376/Manage/SaveInfoMap");
        public Uri ManageSaveInfoReduce = new Uri("https://localhost:44376/Manage/SaveInfoReduce");
        public Uri ManageSaveInfoShuffle = new Uri("https://localhost:44376/Manage/SaveInfoShuffle");
        public Uri ManageShow = new Uri("https://localhost:44376/Manage/Show");
        public Uri ManageClearDB = new Uri("https://localhost:44376/Manage/ClearDataBases");
        public Uri ManageStartShuffle = new Uri("https://localhost:44376/Manage/StartShuffle");

        public Func<Dictionary<string, string>, Dictionary<string, string>> FuncBegin { get; set; }
        public Func<string[], string> FuncResult { get; set; }

        public class FileUpload
        {
            public string FilePath { get; set; }
        }


        public async void Run(string path)
        {
            await ClearDbAsync(ManageClearDB);
            BeginMap(path);
            BeginShuffle();
            BeginReduce();
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
        public void SendFunction(Uri uri, Delegate @delegate)
        {
            using (var client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = uri,

                };
            }
        }
        private void BeginReduce()
        {
            var ports = Retrieve(ManagePort);
            int i = 0;

            foreach (var port in ports)
            {
                Stopwatch sw = Stopwatch.StartNew();
                var data = GetResponseFromURI(new Uri($"{port}GetDataForReduce")).Result;
                foreach (var item in data.Split('|'))
                {
                    string[] values = item.Split('=')[1].Split(';');
                    string key = item.Split('=')[0];
                    var t1 = Task.Run(() => SendDataAsync(new Uri((port + "Reduce").ToString()),
                        $"{key}={RetryReduce(FuncResult, values)}"));
                    t1.Wait();
                    sw.Stop();
                    var t2 = Task.Run(() => SendDataSave(ManageSaveInfoReduce, new List<object> { port, sw.Elapsed }).ToString());
                    t2.Wait();
                }
            }
        }

        private void BeginShuffle()
        {
            var ports = Retrieve(ManagePort);
            List<string> data = new List<string>();


            for (int i = 0; i < ports.Count; i++)
            {
                Stopwatch sw = Stopwatch.StartNew();
                data.Add(GetResponseFromURI(new Uri(ports[i] + "Shuffle")).Result);
                Console.WriteLine($"Shuffle-{ports[i]}");
                sw.Stop();
                //var t2 = Task.Run(() => SendDataSave(ManageSaveInfoShuffle, new List<object> { ports[i % ports.Count], sw.Elapsed }).ToString());
                //t2.Wait();
            }
            var t = Task.Run(() => PostURI(ManageStartShuffle));
            t.Wait();
        }

        private Dictionary<string, List<string>> SortData(List<string> shuffle)
        {
            //var ports = Retrieve(ManagePort);
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
        private void BeginMap(string filePath)
        {
            if (File.Exists(filePath))
            {
                var t = Task.Run(() => SendDataAsync(ManageSaveFilePath, filePath));
                t.Wait();

                List<Dictionary<string, string>> lineValues = new List<Dictionary<string, string>>();
                List<string> lines = new List<string>();
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string[] headerLine = sr.ReadLine().Split(',');

                    while (sr.Peek() != -1)
                    {
                        string line = sr.ReadLine();
                        lines.Add(line);
                    }
                    foreach (var row in lines)
                    {
                        Dictionary<string, string> pairs = new Dictionary<string, string>();
                        for (int j = 0; j < headerLine.Length; j++)
                        {
                            pairs.Add(headerLine[j], row.Split(',')[j]);
                        }
                        lineValues.Add(pairs);
                    }
                }
                var ports = Retrieve(ManagePort);
                for (int i = 0; i < ports.Count; i++)
                {
                    var task = Task.Run(() => SendDataAsync(new Uri((ports[i] + "SaveFileInDB").ToString()), filePath));
                    task.Wait();
                }
                int portIndex = 0;
                foreach (var line in lineValues)
                {
                    Dictionary<string, string> keyValues = (Dictionary<string, string>)RetryMap(FuncBegin, line);
                    var t0 = Task.Run(() => SendDataAsync(ManageSaveInfoLine, lines[portIndex]));
                    t0.Wait();
                    Stopwatch sw = Stopwatch.StartNew();
                    var t1 = Task.Run(() => SendKeyValue(new Uri((ports[portIndex % ports.Count] + "Map").ToString()), keyValues.Keys.First(), keyValues.Values.First()));
                    Console.WriteLine($"Map-{ports[portIndex % ports.Count]}\t{t1.Result}");
                    t1.Wait();
                    sw.Stop();
                    if (t1.Result.ToString() == "OK")
                    {
                        var t2 = Task.Run(() => SendDataSave(ManageSaveInfoMap, new List<object> { ports[portIndex % ports.Count], sw.Elapsed, keyValues.Keys.First(), keyValues.Values.First() }).ToString());
                        t2.Wait();
                    }
                    portIndex++;
                }
            }
        }

        private object RetryMap(Delegate method, object row)
        {
            return method.DynamicInvoke(row);
        }

        private object RetryReduce(Delegate method, object args)
        {
            return method.DynamicInvoke(args);
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

        static async Task<string> PostURI(Uri u)
        {
            var response = string.Empty;
            using (var client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = u
                };

                HttpResponseMessage result = await client.SendAsync(request);
                if (result.IsSuccessStatusCode)
                {
                    response = result.StatusCode.ToString();
                }
            }
            return response;
        }


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
