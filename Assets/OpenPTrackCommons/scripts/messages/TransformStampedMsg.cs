using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace OpenPTrack
{

    ///<author>
    /// Carlo Rizzardo
    ///</author> 
    ///C# representation of the geometry_msgs/TransformStamped message
    ///
    public class TransformStampedMsg : RosSharp.RosBridgeClient.Message
	{
		[JsonIgnore]
		public const string RosMessageName = "geometry_msgs/TransformStamped";
	
		public TimedHeaderMsg header;
		public string child_frame_id;
		public TransformMsg transform;

		public TransformStampedMsg(TransformMsg transform, string child_frame_id, string frame_id)
		{
			header = new TimedHeaderMsg();
			header.frame_id = frame_id;
            header.Update();

			this.transform = transform;
			this.child_frame_id = child_frame_id;
		}

	
		///
		 /// Returns the transform from source_frame to target_frme, if available in this transform
		 /// @param target_frame the target frame of the transform to be returned
		 /// @param source_frame the source frame of the transform to be returned
		 /// @return the transform, null if not available
		 ///
		public TransformMsg getTransform(string target_frame, string source_frame)
		{
			source_frame = Utils.cleanFrameId(source_frame);
			target_frame = Utils.cleanFrameId(target_frame);

			//OptarLogger.info("getting transform from message with "+header.frame_id+" to "+child_frame_id);

			TransformMsg ret = null;
			if(source_frame.Equals(Utils.cleanFrameId(header.frame_id)) && 
				target_frame.Equals(Utils.cleanFrameId(child_frame_id)))
			{
				//then we have the transform
				ret = transform;
			}
			else if(target_frame.Equals(Utils.cleanFrameId(header.frame_id)) && 
					source_frame.Equals(Utils.cleanFrameId(child_frame_id)))
			{
				//then we have the inverse of the transform
				Matrix4x4 m = Matrix4x4.TRS(transform.getTranslationVector3(),
											transform.getOrientation(),
											new Vector3(1,1,1));

				Vector3 position = m.inverse.GetColumn(3); 
				Quaternion rotation = Quaternion.LookRotation(m.inverse.GetColumn(2), m.inverse.GetColumn(1));
				
				ret = new TransformMsg(position, rotation);
			}

			return ret;
		}
	}
}




