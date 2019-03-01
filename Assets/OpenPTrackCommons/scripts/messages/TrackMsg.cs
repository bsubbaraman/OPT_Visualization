using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace OpenPTrack
{

    ///<author>
    /// Carlo Rizzardo
    ///</author>
    ///C# representation of the opt_msgs/Track message
    ///
    public class TrackMsg : RosSharp.RosBridgeClient.Message
	{
		[JsonIgnore]
		public const string RosMessageName = "opt_msgs/Track";

		public byte VISIBLE = 0;
		public byte OCCLUDED = 1;
		public byte NOT_VISIBLE = 2;

		public int id;

		public double x;
		public double y;
		public double height;
		public double distance;
		public double age;
		public double confidence;

		public byte visibility;

		public int stable_id;

		public BoundingBox2DMsg box_2D;

		public string object_name;
		public string face_name;
		
	}
}