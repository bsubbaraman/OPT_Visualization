using Newtonsoft.Json;

namespace OpenPTrack
{
    public class OptarPoseStampedMsg : RosSharp.RosBridgeClient.Message
    {
        [JsonIgnore]
        public const string RosMessageName = "geometry_msgs/PoseStamped";
        public TimedHeaderMsg header;
        public RosSharp.RosBridgeClient.Messages.Geometry.Pose pose;

        public OptarPoseStampedMsg(RosSharp.RosBridgeClient.Messages.Geometry.Point position, RosSharp.RosBridgeClient.Messages.Geometry.Quaternion orientation, string frameName)
        {
            header = new TimedHeaderMsg();
            header.Update();
            
            header.frame_id = frameName;
            pose = new RosSharp.RosBridgeClient.Messages.Geometry.Pose();
            pose.position = position;
            pose.orientation = orientation;
        }


        private static RosSharp.RosBridgeClient.Messages.Geometry.Point unityVectorToPoint(UnityEngine.Vector3 vec)
        {
            RosSharp.RosBridgeClient.Messages.Geometry.Point point = new RosSharp.RosBridgeClient.Messages.Geometry.Point();
            point.x = vec.x;
            point.y = vec.y;
            point.z = vec.z;
            return point;
        }
        private static RosSharp.RosBridgeClient.Messages.Geometry.Quaternion unityQuaternionToRos(UnityEngine.Quaternion unityQuaternion)
        {
            RosSharp.RosBridgeClient.Messages.Geometry.Quaternion quaternion = new RosSharp.RosBridgeClient.Messages.Geometry.Quaternion();
            quaternion.x = unityQuaternion.x;
            quaternion.y = unityQuaternion.y;
            quaternion.z = unityQuaternion.z;
            quaternion.w = unityQuaternion.w;
            return quaternion;
        }
        public OptarPoseStampedMsg(UnityEngine.Vector3 position, UnityEngine.Quaternion orientation, string frameName) :
         this(unityVectorToPoint(position),unityQuaternionToRos(orientation),frameName)
        {
            
        }

        public OptarPoseStampedMsg(RosSharp.RosBridgeClient.Messages.Geometry.Pose pose, string frameName) : this(pose.position,pose.orientation,frameName)
        {
        
        }
    }
}