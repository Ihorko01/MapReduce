using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageNode.Models
{
    public class MapFile
    {
        public int Id { get; set; }
        public string Datas { get; set; }
        public string Node { get; set; }
        public int? ShufflesId { get; set; }
    }
}
