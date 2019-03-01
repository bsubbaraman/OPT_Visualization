using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace OpenPTrack
{

    ///<author>
    /// Carlo Rizzardo
    ///</author>
    ///C# representation of the opt_msgs/TrackArray message
    ///
    public class Track3DMsg : RosSharp.RosBridgeClient.Message
	{
		[JsonIgnore]
		public const string RosMessageName = "opt_msgs/Track3D";
		public byte VISIBLE = 0;
		public byte OCCLUDED = 1;
		public byte NOT_VISIBLE = 2;

		public int id;

		public double x;
		public double y;
		public double z;
		public double height;
		public double distance;
		public double age;
		public double confidence;

		public byte visibility;

		public BoundingBox2DMsg box_2D;		
	}
}


