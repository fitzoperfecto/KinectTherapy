using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace Microsoft.Samples.Kinect.XnaBasics
{
    /// <summary>
    /// Provides a collection of the joins measured a boolean of whether it was in range,
    /// and the difference between the current and original for each point
    /// </summary>
    class FormResult
    {
        public JointType JointType { get; set; }
        public bool InRange {get;set;}
        public Dictionary<String, float> PointVariance { get; set; }

        public FormResult()
        {
            InRange = true;
            PointVariance = new Dictionary<String,float>();
        }
        /// <summary>
        /// This just provides a little protection from getting the result into a bad state. 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="variance"></param>
        public void addPointVariance(String point, float variance)
        {
            if (!"X".Equals(point) && !"Y".Equals(point) && !"Z".Equals(point))
            {
                throw new Exception("point must be X, Y or Z");
            }

            PointVariance.Add(point, variance);
        }
    }
}
