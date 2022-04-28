using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataNode3.Models
{
    public class SubPart
    {
        public int Id { get; set; }
        public string Sub { get; set; }
        public int FileId { get; set; }
        public int? MapId { get; set; }
        public int? ShuffleId { get; set; }
    }
}
