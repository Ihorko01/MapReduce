using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Library
{
    public class Methods
    {
        public class FileUpload
        {
            public string FilePath { get; set; }
        }
        public void Send(string path)
        {
            var t = Task.Run(() => GetResponseFromURI(new Uri($"https://localhost:44322/Manage/GetFile?path={path}")));
            Console.WriteLine(t.Result);
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
        public void Retrieve() 
        { }
        public void Run() 
        { }
    }
}
