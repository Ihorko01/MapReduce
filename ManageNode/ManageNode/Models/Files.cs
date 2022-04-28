using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageNode.Models
{
    public class Files
    {
        public int Id { get; set; }
        public string Subs { get; set; }
        public string Maps { get; set; }
        public string MapNode { get; set; }
        public string Shuffles { get; set; }
        public string Sorts { get; set; }
        public string Reduces { get; set; }
        public string ReduceNode { get; set; }
    }
}
