using ManageNode.Models;
using ManageNode.Models;
using ManageNode.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace ManagmentServer.Controllers
{
    public class FileController : Controller
    {
        private AppDbContext db;
        public FileController(AppDbContext db)
        {
            this.db = db;
        }

        public ActionResult Index()
        {
            List<FileViewModel> fileView = new List<FileViewModel>();
            foreach (var item in db.Statistics)
            {
                fileView.Add(new FileViewModel { Node = item.Node, CountMap = item.CountMap.ToString(), TimeMap = item.TimeMap, CountShuffle = item.CountShuffle.ToString(), TimeShuffle = item.TimeShuffle, CountReduce = item.CountReduce.ToString(), TimeReduce = item.TimeReduce });
            }
            db.SaveChanges();

            List<PieViewModel> dataPieMap = new List<PieViewModel>();
            List<PieViewModel> dataPieShuffle = new List<PieViewModel>();
            List<PieViewModel> dataPieReduce = new List<PieViewModel>();
            List<PieViewModel> dataLineTime1 = new List<PieViewModel>();
            List<PieViewModel> dataLineTime2 = new List<PieViewModel>();
            List<PieViewModel> dataLineTime3 = new List<PieViewModel>();

            List<PieViewModel> avarageLineTime1 = new List<PieViewModel>();
            List<PieViewModel> avarageLineTime2 = new List<PieViewModel>();
            List<PieViewModel> avarageLineTime3 = new List<PieViewModel>();

            List<PieViewModel> avaregeTime = new List<PieViewModel>();
            foreach (var item in fileView)
            {
                dataPieMap.Add(new PieViewModel(item.Node, Convert.ToDouble(item.CountMap)));
                dataPieShuffle.Add(new PieViewModel(item.Node, Convert.ToDouble(item.CountShuffle)));
                dataPieReduce.Add(new PieViewModel(item.Node, Convert.ToDouble(item.CountReduce)));
            }
            var x = Convert.ToDateTime(fileView[0].TimeMap).TimeOfDay.TotalSeconds;

            dataLineTime1.Add(new PieViewModel("Map", Convert.ToDateTime(fileView[0].TimeMap).TimeOfDay.TotalSeconds));
            dataLineTime1.Add(new PieViewModel("Shuffle", Convert.ToDateTime(fileView[0].TimeShuffle).TimeOfDay.TotalSeconds));
            dataLineTime1.Add(new PieViewModel("Reduce", Convert.ToDateTime(fileView[0].TimeReduce).TimeOfDay.TotalSeconds));

            dataLineTime2.Add(new PieViewModel("Map", Convert.ToDateTime(fileView[1].TimeMap).TimeOfDay.TotalSeconds));
            dataLineTime2.Add(new PieViewModel("Shuffle", Convert.ToDateTime(fileView[1].TimeShuffle).TimeOfDay.TotalSeconds));
            dataLineTime2.Add(new PieViewModel("Reduce", Convert.ToDateTime(fileView[1].TimeReduce).TimeOfDay.TotalSeconds));

            //dataLineTime3.Add(new PieViewModel("Map", Convert.ToDateTime(fileView[2].TimeMap).TimeOfDay.TotalSeconds));
            //dataLineTime3.Add(new PieViewModel("Shuffle", Convert.ToDateTime(fileView[1].TimeShuffle).TimeOfDay.TotalSeconds));
            //dataLineTime3.Add(new PieViewModel("Reduce", Convert.ToDateTime(fileView[1].TimeReduce).TimeOfDay.TotalSeconds));

            avarageLineTime1.Add(new PieViewModel("Map", Convert.ToDateTime(fileView[0].TimeMap).TimeOfDay.TotalSeconds / Convert.ToDouble(fileView[0].CountMap)));
            avarageLineTime1.Add(new PieViewModel("Shuffle", Convert.ToDateTime(fileView[0].TimeShuffle).TimeOfDay.TotalSeconds / Convert.ToDouble(fileView[0].CountShuffle)));
            avarageLineTime1.Add(new PieViewModel("Reduce", Convert.ToDateTime(fileView[0].TimeReduce).TimeOfDay.TotalSeconds / Convert.ToDouble(fileView[0].CountReduce)));

            avarageLineTime2.Add(new PieViewModel("Map", Convert.ToDateTime(fileView[1].TimeMap).TimeOfDay.TotalSeconds / Convert.ToDouble(fileView[1].CountMap)));
            avarageLineTime2.Add(new PieViewModel("Shuffle", Convert.ToDateTime(fileView[1].TimeShuffle).TimeOfDay.TotalSeconds / Convert.ToDouble(fileView[1].CountShuffle)));
            avarageLineTime2.Add(new PieViewModel("Reduce", Convert.ToDateTime(fileView[1].TimeReduce).TimeOfDay.TotalSeconds / Convert.ToDouble(fileView[1].CountReduce)));

            ViewBag.Nodes = JsonConvert.SerializeObject(fileView.Select(s => s.Node).ToList());
            ViewBag.LinePoints1 = JsonConvert.SerializeObject(dataLineTime1);
            ViewBag.LinePoints2 = JsonConvert.SerializeObject(dataLineTime2);
            ViewBag.LineAvarage1 = JsonConvert.SerializeObject(avarageLineTime1);
            ViewBag.LineAvarage2 = JsonConvert.SerializeObject(avarageLineTime2);

            ViewBag.DataPointsMap = JsonConvert.SerializeObject(dataPieMap);
            ViewBag.DataPointsShuffle = JsonConvert.SerializeObject(dataPieShuffle);
            ViewBag.DataPointsReduce = JsonConvert.SerializeObject(dataPieReduce);

            return View(fileView);
        }

        private List<TransformingPartViewModel> GetTransformingPartViews()
        {
            string[] ports = new string[]
            {
                    ConfigurationManager.AppSettings["datanode1"].ToString(),
                    ConfigurationManager.AppSettings["datanode2"].ToString(),
                    ConfigurationManager.AppSettings["datanode3"].ToString()
            };
            int fileId = db.Files.Max(f => f.Id);
            var lines = db.Lines.Where(l => l.FileId == fileId).ToList();
            var maps = new List<MapFile>();
            List<TransformingPartViewModel> viewModels = new List<TransformingPartViewModel>();
            for (int i = 0; i < lines.Count; i++)
            {
                maps.Add(db.Map.Where(m => m.LineId == lines[i].Id).FirstOrDefault());
            }
            var shuffle = new List<Shuffle>();
            return viewModels;
        }

        //    [HttpPost]
        //    public void GetDataFromMap([FromBody] string data)
        //    {
        //        AddMapFile(new MapFile { Datas = data });
        //    }
        //    [HttpPost]
        //    public void GetDataFromShuffle([FromBody] string data)
        //    {
        //        AddShuffleFile(new Shuffle { Data = data });
        //    }
        //    [HttpPost]
        //    public void GetDataFromReduce([FromBody] string data)
        //    {
        //        AddReduceFile(new ReduceFile { Datas = data });
        //    }
        //    [HttpPost]
        //    public string AddFile([FromBody]string fileName)
        //    {
        //        MainFile mainFile = new MainFile { Name = fileName };

        //        db.MainFile.Add(mainFile);
        //        db.SaveChanges();
        //        return "Success";
        //    }

        //    [HttpPost]
        //    public void AddSubFile([FromBody]List<string> list)
        //    {
        //        string data = list[0];
        //        string node = list[1];
        //        int fileId = db.MainFile.Max(item => item.Id);
        //        MainFile main = db.MainFile.Find(fileId);
        //        MapFile map = new MapFile { Node = node };
        //        db.MapFiles.Add(map);
        //        db.SaveChanges();
        //        var subFile = new SubFile {
        //            MapId = map.Id,
        //            Data = data,
        //            FileId = main.Id};

        //        db.SubFiles.Add(subFile);
        //        db.SaveChanges();
        //    }

        //    [HttpPost]
        //    public void AddMapFile(MapFile mapFile)
        //    {
        //        int fileId = db.MapFiles.Max(item => item.Id);
        //        MapFile mFile = db.MapFiles.Find(fileId);
        //        mFile.Datas = mapFile.Datas;
        //        db.MapFiles.Update(mFile);
        //        db.SaveChanges();
        //    }

        //    [HttpPost]
        //    public void AddShuffleFile(Shuffle shuffle)
        //    {
        //        var shuffleFile = db.Shuffles.Find(db.Shuffles.Max(item => item.Id));
        //        shuffleFile.Data = shuffle.Data;
        //        //db.Shuffles.AddRange(shuffle);
        //        db.SaveChanges();
        //    }

        //    [HttpPost]
        //    public void AddReduceFile(ReduceFile reduceFile)
        //    {
        //        var reduce = db.ReduceFiles.Find(db.ReduceFiles.Max(item => item.Id));
        //        reduce.Datas = reduceFile.Datas;
        //        db.SaveChanges();
        //    }
        //}
    }
}
