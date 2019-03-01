using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace OpenPTrack
{
    /// <author>
    /// Carlo Rizzardo
    /// </author>
    /// <summary>
    /// C# representation of the opt_msgs/BoundingBox2D message
    /// </summary>
    public class BoundingBox2DMsg : RosSharp.RosBridgeClient.Message
	{
		[JsonIgnore]
		public const string RosMessageName = "opt_msgs/BoundingBox2D";
		public int x;
		public int y;
		public int width;
		public int height;
		
	}
}

