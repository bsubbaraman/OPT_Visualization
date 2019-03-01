using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace OpenPTrack
{

    ///<author>
    /// Carlo Rizzardo
    ///</author>
    ///C# representation of the geometry_msgs/Transform message
    ///
    public class TransformMsg : RosSharp.RosBridgeClient.Message
	{
		[JsonIgnore]
		public const string RosMessageName = "geometry_msgs/Transform";
	
		public Vector3Msg translation;
        public RosSharp.RosBridgeClient.Messages.Geometry.Quaternion rotation;

		public TransformMsg()
		{
            translation = new Vector3Msg();
            rotation = new RosSharp.RosBridgeClient.Messages.Geometry.Quaternion();
            this.rotation.x = 1;
            this.rotation.y = 0;
            this.rotation.z = 0;
            this.rotation.w = 0;
        }

		public TransformMsg(Vector3Msg translation, Quaternion orientation)
		{
			this.translation = translation;
			this.rotation = new RosSharp.RosBridgeClient.Messages.Geometry.Quaternion();
			this.rotation.x=orientation.x;
			this.rotation.y=orientation.y;
			this.rotation.z=orientation.z;
			this.rotation.w=orientation.w;
		}

		public TransformMsg(Vector3 translation, Quaternion orientation) : this(new Vector3Msg(translation.x,translation.y,translation.z),orientation)
		{
		}

		public TransformMsg(TransformStampedMsg transformStamped) : this(new Vector3Msg(transformStamped.transform.translation.x,
														transformStamped.transform.translation.y,
														transformStamped.transform.translation.z),
											new Quaternion(transformStamped.transform.rotation.x,
															transformStamped.transform.rotation.y,
															transformStamped.transform.rotation.z,
															transformStamped.transform.rotation.w))
		{}

		public Vector3 getTranslationVector3()
		{
			return new Vector3((float)translation.x, (float)translation.y, (float)translation.z);
		}

		public Quaternion getOrientation()
		{
			return new Quaternion(rotation.x,rotation.y,rotation.z,rotation.w);
		}
	}
}
