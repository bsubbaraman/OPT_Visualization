using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace OpenPTrack
{
    ///<author>
    /// Carlo Rizzardo
    ///</author>
    ///ROS message used to exchange time information between a ROS node and
    ///an android device that uses optar
    ///
    public class OptarNtpMessageMsg : RosSharp.RosBridgeClient.Message
	{
		[JsonIgnore]
		public const string RosMessageName = "opt_msgs/OptarNtpMessage";
		///
		///The header
		///
		public TimedHeaderMsg header = new TimedHeaderMsg();

		///
		/// The type of the message. Either a request for the time or a reply to it
		///
		public int type;
		public const int QUERY = 0;
		public const int REPLY = 1;


		///
		/// This id is needed to match a reply to its query. The server will use the query's id
		/// as the id for the reply. It should be unique among all the messages exchanged in the
		/// current ROS network. It's the client's responsibility to choose it.
		///
		public string id;

		///
		/// The time on the server side. This will be filled up by the server when generating a reply
		///
		public RosSharp.RosBridgeClient.Messages.Standard.Time serverTime;
		
		///
		/// The time on the client side at the time of the request
		///
		public RosSharp.RosBridgeClient.Messages.Standard.Time clientRequestTime;
		
		public OptarNtpMessageMsg(int type)
		{
			this.type = type;
			
			//generate a unique id
			System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
			System.TimeSpan cur_time = System.DateTime.UtcNow - epochStart;
			this.id = ""+cur_time.TotalMilliseconds + "-" + (new System.Random()).Next();//this should be quite unique

			//set the clientRequestTime to this moment
			clientRequestTime = Utils.getRosTimeNow();
		}

	}
}