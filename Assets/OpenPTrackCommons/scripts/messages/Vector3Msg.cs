using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace OpenPTrack
{
    ///<author>
    /// Carlo Rizzardo
    ///</author>
    ///<summary>
    ///C# representation of the geometry_msgs/Vector3 message
    ///</summary>
    public class Vector3Msg : RosSharp.RosBridgeClient.Message
	{
		[JsonIgnore]
		public const string RosMessageName = "geometry_msgs/Vector3";
	
		
        public double x;
        public double y;
        public double z;

		public Vector3Msg()
		{
            x = 0;
            y = 0;
            z = 0;
		}

        public Vector3Msg(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public Vector3Msg(Vector3 point)
        {
            this.x = point.x;
            this.y = point.y;
            this.z = point.z;
        }

        public Vector3 asVector3()
        {
            return new Vector3((float)x, (float)y, (float)z);
        }
	}
}
