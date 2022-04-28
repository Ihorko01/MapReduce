using ManageNode.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections;
using Newtonsoft.Json;
using ManageNode.ViewModel;
using System.Net;
using Microsoft.IdentityModel.Protocols;

namespace ManagmentServer.Controllers
{
    public class ManageController : Controller
    {
        private AppDbContext db;
        private bool existInTable;

        public ManageController(AppDbContext db)
        {
            this.db = db;
        }
        [HttpPost]

        public void SaveInfoMap([FromBody] List<object> Info)
        {
            existInTable = db.Statistics.Any(item => item.Node == Info[0].ToString());
            if (existInTable)
            {
                Statistics statistics = db.Statistics.Where(item => item.Node == Info[0].ToString()).FirstOrDefault();
                statistics.TimeMap = TimeSpan.Parse(Info[1].ToString()).Add(TimeSpan.Parse(statistics.TimeMap)).ToString();
                statistics.CountMap += 1;
                db.SaveChanges();
            }
            else
            {
                db.Statistics.Add(new Statistics { Node = Info[0].ToString(), CountMap = 1, TimeMap = Info[1].ToString(), CountReduce = 0, CountShuffle = 0 });
                db.SaveChanges();
            }
        }

        [HttpPost]
        public void SaveInfoShuffle([FromBody] List<object> Info)
        {
            Statistics statistics = db.Statistics.Where(item => item.Node == Info[0].ToString()).FirstOrDefault();
            if (statistics.CountShuffle != 0)
            {
                statistics.CountShuffle += 1;
                statistics.TimeShuffle = TimeSpan.Parse(Info[1].ToString()).Add(TimeSpan.Parse(statistics.TimeShuffle)).ToString();
                db.SaveChanges();
            }
            else
            {
                statistics.CountShuffle += 1;
                statistics.TimeShuffle = Info[1].ToString();
                db.SaveChanges();
            }
        }

        [HttpPost]
        public void SaveInfoReduce([FromBody] List<object> Info)
        {
            Statistics statistics = db.Statistics.Where(item => item.Node == Info[0].ToString()).FirstOrDefault();
            if (statistics.CountReduce != 0)
            {
                statistics.CountReduce += 1;
                statistics.TimeReduce = TimeSpan.Parse(Info[1].ToString()).Add(TimeSpan.Parse(statistics.TimeReduce)).ToString();
                db.SaveChanges();
            }
            else
            {
                statistics.CountReduce += 1;
                statistics.TimeReduce = Info[1].ToString();
                db.SaveChanges();
            }
        }

        [HttpPost]
        public void ClearDataBases()
        {
            var records = from m in db.Statistics
                          select m;
            foreach (var record in records)
            {
                db.Statistics.Remove(record);
            }
            db.SaveChanges();
        }

        public ActionResult Index()
        {
            List<FileViewModel> fileView = new List<FileViewModel>();
            foreach (var item in db.Statistics)
            {
                fileView.Add(new FileViewModel { Node = item.Node, CountMap = item.CountMap.ToString(), TimeMap = item.TimeMap, CountShuffle = item.CountShuffle.ToString(), TimeShuffle = item.TimeShuffle, CountReduce = item.CountReduce.ToString(), TimeReduce = item.TimeReduce });
            }
            db.Statistics.RemoveRange(db.Statistics);
            db.SaveChanges();

            return Json(fileView);
        }

        public ActionResult Map()
        {
            List<FileViewModel> fileView = new List<FileViewModel>();
            foreach (var item in db.Statistics)
            {
                fileView.Add(new FileViewModel { Node = item.Node, CountMap = item.CountMap.ToString(), TimeMap = item.TimeMap });
            }
            return View(fileView);
        }

        [HttpGet]
        public string[] GetPorts()
        {

            string[] allPorts = new string[]
            {
                    ConfigurationManager.AppSettings["datanode1"].ToString(),
                    ConfigurationManager.AppSettings["datanode2"].ToString(),
                    ConfigurationManager.AppSettings["datanode3"].ToString()
            };
            List<string> healthyPorts = new List<string>();
            for (int i = 0; i < allPorts.Length; i++)
            {

                HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(new Uri("https://localhost:" + new Uri(allPorts[i]).Port));
                httpReq.AllowAutoRedirect = false;
                try
                {
                    HttpWebResponse httpRes = (HttpWebResponse)httpReq.GetResponse();
                    if (httpRes.StatusCode == HttpStatusCode.OK)
                    {
                        healthyPorts.Add(allPorts[i]);
                    }
                    httpRes.Close();
                }
                catch (Exception)
                {

                }

            }
            return healthyPorts.ToArray();
        }
    }
}
