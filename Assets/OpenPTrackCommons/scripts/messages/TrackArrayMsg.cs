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
    public class TrackArrayMsg : RosSharp.RosBridgeClient.Message
	{
		[JsonIgnore]
		public const string RosMessageName = "opt_msgs/TrackArray";
		public TimedHeaderMsg header;

		public TrackMsg[] tracks;
		
	}
}