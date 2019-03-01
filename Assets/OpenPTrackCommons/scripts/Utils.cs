using System.Diagnostics;
using System;
using UnityEngine;

namespace OpenPTrack
{
    /// <summary>
    /// Utilities Ccollection for OpenPTrack Unity Projects
    /// </summary>
    /// <author>
    /// Carlo Rizzardo
    /// </author>
    public class Utils
    {
        public static RosSharp.RosBridgeClient.Messages.Standard.Time getRosTimeNow()
        {
            RosSharp.RosBridgeClient.Messages.Standard.Time time = new RosSharp.RosBridgeClient.Messages.Standard.Time();
            System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            System.TimeSpan cur_time = System.DateTime.UtcNow - epochStart;

            //set the clientRequestTime to this moment
            int secs = (int)cur_time.TotalSeconds;
            int nsecs = (int)((cur_time.TotalMilliseconds - secs*1000.0)*1000000);
            time.secs = secs;
            time.nsecs = nsecs;
            return time;
        }
        public static RosSharp.RosBridgeClient.Messages.Standard.Time getRosTime(double timeSecs)
        {
            System.TimeSpan cur_time = new System.TimeSpan(0,0,0,(int)timeSecs,(int)((timeSecs-(int)timeSecs)*1000));

            //set the clientRequestTime to this moment
            int secs = (int)cur_time.TotalSeconds;
            int nsecs = (int)((cur_time.TotalMilliseconds - secs*1000.0)*1000000);

            RosSharp.RosBridgeClient.Messages.Standard.Time time = new RosSharp.RosBridgeClient.Messages.Standard.Time();
            time.secs = secs;
            time.nsecs = nsecs;
            return time;
        }

        public static long rosTimeToMillis(RosSharp.RosBridgeClient.Messages.Standard.Time rosTime)
        {
            return rosTime.secs*1000 + rosTime.nsecs/1000000;
        }
        
        public static long rosTimeToMicro(RosSharp.RosBridgeClient.Messages.Standard.Time rosTime)
        {
            return rosTime.secs*1000000 + rosTime.nsecs/1000;
        }

        public static RosSharp.RosBridgeClient.Messages.Geometry.Pose unityPoseToRosPose(UnityEngine.Pose unityPose)
        {
            RosSharp.RosBridgeClient.Messages.Geometry.Pose rosPose = new RosSharp.RosBridgeClient.Messages.Geometry.Pose();

            rosPose.position = new RosSharp.RosBridgeClient.Messages.Geometry.Point();
            rosPose.position.x = unityPose.position.x;
            rosPose.position.y = -unityPose.position.y;
            rosPose.position.z = unityPose.position.z;

            rosPose.orientation = new RosSharp.RosBridgeClient.Messages.Geometry.Quaternion();
            rosPose.orientation.x = unityPose.rotation.x;
            rosPose.orientation.y = -unityPose.rotation.y;
            rosPose.orientation.z = unityPose.rotation.z;
            rosPose.orientation.w = -unityPose.rotation.w;

            return rosPose;
        }


        public static long getTimeNano()
        {
            System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
			System.DateTime now = System.DateTime.UtcNow;
			now = now + (new System.TimeSpan(0,0,0,0,(int)(NtpClient.singleton.getEstimatedTimeDiffMicro()/1000)));
			//OptarLogger.info("time correction = "+(new System.TimeSpan(0,0,0,0,(int)(NtpClient.singleton.getEstimatedTimeDiffMicro()/1000))));
			System.TimeSpan cur_time = now - epochStart;

			//OptarLogger.info("TotalSeconds = "+cur_time.TotalSeconds+"   TotalMilliseconds = "+cur_time.TotalMilliseconds);
			return ((long)cur_time.TotalMilliseconds)*1000000L;


            /*
            long nano = 10000L * Stopwatch.GetTimestamp();
            nano /= TimeSpan.TicksPerMillisecond;
            nano *= 100L;
            return nano;
            */
        }

        public static long toTimeNano( RosSharp.RosBridgeClient.Messages.Standard.Time time)
		{
			return (long)time.secs * 1000000000L + time.nsecs;
		}

        public static long toTimeMillis( RosSharp.RosBridgeClient.Messages.Standard.Time time)
		{
			return (long)time.secs * 1000L + time.nsecs/1000000L;
		}

        public static double toTimeToSecs(RosSharp.RosBridgeClient.Messages.Standard.Time time)
        {
            return ((double)toTimeNano(time)) / 1000000000L;
        }

        /**
         * Cleans the provided frame id form whitespace and make fixes the slashes usage
         * @param frame_id the frame id to clean
         *
         * @return the cleaned frame id
         */
        public static string cleanFrameId(string frame_id)
		{
			//remmove spaces
			frame_id = frame_id.Replace(" ", "");

			//remove initial slash
			if(frame_id[0] == '/')
			{
				frame_id = frame_id.Substring(1);
			}

			return frame_id;
		}



        public static Vector3 rosToUnity(Vector3 rosVector)
        {
            return new Vector3(rosVector.x, -rosVector.y, rosVector.z);
        }

        public static Vector3 rosToUnity(Vector3Msg rosVector)
        {
            Vector3 rosVectorCasted = rosVector.asVector3();
            return rosToUnity(rosVectorCasted);
        }

        public static Vector3Msg unityToRos(Vector3 rosVector)
        {
            return new Vector3Msg(rosVector.x, -rosVector.y, rosVector.z);
        }


        public static Quaternion rosToUnity(Quaternion rosQuaternion)
        {
            float rightHandedAngle;
            Vector3 rightHandedAxis;
            rosQuaternion.ToAngleAxis(out rightHandedAngle,out rightHandedAxis);

            return Quaternion.AngleAxis(-rightHandedAngle, rosToUnity(rightHandedAxis));
        }
    }
}