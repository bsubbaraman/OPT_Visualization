using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace OpenPTrack
{
	///
	/// Carlo Rizzardo
	///
	///C# representation of the std_msgs/ColorRGBA message
	///
	public class ColorRGBAMsg : RosSharp.RosBridgeClient.Message
	{
		[JsonIgnore]
		public const string RosMessageName = "std_msgs/ColorRGBA";
				
		public float r;
		public float g;
		public float b;
		public float a;
		
	}
}

