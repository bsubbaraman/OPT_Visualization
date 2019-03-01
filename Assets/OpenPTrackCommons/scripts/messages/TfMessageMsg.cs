using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace OpenPTrack
{

    ///<author>
    /// Carlo Rizzardo
    ///</author>
    ///C# representation of the tf/tfMessage message
    ///
    public class TfMessageMsg : RosSharp.RosBridgeClient.Message
	{
		[JsonIgnore]
		public const string RosMessageName = "tf2_msgs/TFMessage";
	
		public TransformStampedMsg[] transforms;
		public TfMessageMsg(TransformStampedMsg[] transformStamped)
		{
			this.transforms = transformStamped;
		}
	}
}
