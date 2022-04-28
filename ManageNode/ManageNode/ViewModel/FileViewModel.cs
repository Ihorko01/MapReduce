using ManageNode.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ManageNode.ViewModel
{
    public class FileViewModel
    {
        public string Node { get; set; }
        public string CountMap { get; set; }
        public string TimeMap { get; set; }
        public string CountShuffle { get; set; }
        public string TimeShuffle { get; set; }
        public string TimeReduce { get; set; }
        public string CountReduce { get; set; }
    }
}
