using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManagmentServer.Controllers
{
    public class ManageController : Controller
    {
        public ActionResult Index()
        {
            return View(); 
        }
        public ActionResult ShowData()
        {
            return View();
        }
        CancellationTokenSource cts = new CancellationTokenSource();

        [HttpGet]
        public async Task<string> GetFile(string path)
        {
            Manage(path);
            return $"https://localhost:44396";
        }

        [HttpPost]
        public async Task<ActionResult> Manage(string path)
        {

            if (System.IO.File.Exists(path))
            {
                string readText = System.IO.File.ReadAllText(path);
                int blockLength = readText.Length / 4;
                List<string> Blocks = new List<string>(readText.Length / blockLength+1);
                for (int i = 0; i < readText.Length; i += blockLength)
                {
                    if (readText.Length - i > blockLength)
                        Blocks.Add(readText.Substring(i, blockLength));
                    else
                        Blocks.Add(readText.Substring(i, readText.Length - i) + new String('\0', blockLength - (readText.Length - i)));
                }
                await GetAsync(Blocks[0], cts.Token).ConfigureAwait(false);
            }
            return RedirectToAction("Index");
        }

        [NonAction]
        public async Task<string> GetAsync(string text, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            Uri requestUrl = new Uri($"https://localhost:5001/MapReducer/MapReducer?text={text}");
            HttpClient httpClient = new HttpClient();

            var response = await httpClient.GetAsync(requestUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return data.ToString();
        }
    }
}
