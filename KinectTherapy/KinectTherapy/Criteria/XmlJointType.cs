using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace SWENG.Criteria
{
    /// <summary>
    /// Wrapper for the Kinect JointType enumeration
    /// </summary>
    [Serializable()]
    public class XmlJointType 
    {
        /// <summary>
        /// JointString can be set to one of the twenty enumerations for JointType
        /// HipCenter = 0,
        /// Spine = 1,
        /// ShoulderCenter = 2,
        /// Head = 3,
        /// ShoulderLeft = 4,
        /// ElbowLeft = 5,
        /// WristLeft = 6,
        /// HandLeft = 7,
        /// ShoulderRight = 8,
        /// ElbowRight = 9,
        /// WristRight = 10,
        /// HandRight = 11,
        /// HipLeft = 12,
        /// KneeLeft = 13,
        /// AnkleLeft = 14,
        /// FootLeft = 15,
        /// HipRight = 16,
        /// KneeRight = 17,
        /// AnkleRight = 18,
        /// FootRight = 19,
        /// </summary>
        [System.Xml.Serialization.XmlElement("Name")]
        public string Name { get; set; }

        /// <summary>
        /// Empty Constructor Needed for XmlSerializer
        /// </summary>
        public XmlJointType()
        {
        }

        public XmlJointType(string Name)
        {
            this.Name = Name;
        }

        /// <summary>
        /// Uses enumeration helper method to cast the string to the actual enumeration. 
        /// </summary>
        /// <returns></returns>
        public JointType GetJointType()
        {
            return (JointType) Enum.Parse(typeof(JointType), Name);
        }
    }
}
