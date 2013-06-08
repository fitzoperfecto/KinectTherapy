using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SWENG.Criteria
{
    [Serializable()]
    [XmlRoot("Checkpoint")]
    public class Checkpoint
    {
        [XmlAttribute("Sequence")]
        public int Sequence { get; set; }
        [XmlArray("Criteria")]
        [XmlArrayItem("Criterion")]
        public Criterion[] Criteria;

        public Checkpoint()
        {
        }

    }
}
