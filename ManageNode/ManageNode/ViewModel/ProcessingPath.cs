using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageNode.ViewModel
{
    public class ProcessingPath
    {
        public string Part { get; set; }
        public string MapNode { get; set; }
        public string ShuffleNode { get; set; }
        public string ReduceNode { get; set; }
    }
}
