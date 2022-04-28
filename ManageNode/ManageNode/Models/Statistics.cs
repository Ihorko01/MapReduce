using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageNode.Models
{
    public class Statistics
    {
        public int Id { get; set; }
        public string Node { get; set; }
        public int CountMap { get; set; }
        public string TimeMap { get; set; }
        public int CountShuffle { get; set; }
        public string TimeShuffle { get; set; }
        public int CountReduce { get; set; }
        public string TimeReduce { get; set; }
    }
}
