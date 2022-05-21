using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace ManageNode.ViewModel
{
    [DataContract]
    public class PieViewModel
    {
        public PieViewModel(string node, double data)
        {
            Node = node;
            Data = data;
        }

        [DataMember(Name = "label")]
        public string Node { get; set; }

        [DataMember(Name = "y")]
        public double Data { get; set; }
    }
}
