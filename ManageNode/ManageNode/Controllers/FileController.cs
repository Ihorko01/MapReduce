using ManageNode.Models;
using ManageNode.Models;
using ManageNode.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
            db.Statistics.RemoveRange(db.Statistics);
            db.SaveChanges();

            return View(fileView);
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
