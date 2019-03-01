using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenPTrack
{
	public class TimedHeaderMsg : RosSharp.RosBridgeClient.Messages.Standard.Header
	{
		public void Update()
		{
			System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
			System.DateTime now = System.DateTime.UtcNow;
			now = now + (new System.TimeSpan(0,0,0,0,(int)(NtpClient.singleton.getEstimatedTimeDiffMicro()/1000)));
			//OptarLogger.info("time correction = "+(new System.TimeSpan(0,0,0,0,(int)(NtpClient.singleton.getEstimatedTimeDiffMicro()/1000))));
			System.TimeSpan cur_time = now - epochStart;

			//OptarLogger.info("TotalSeconds = "+cur_time.TotalSeconds+"   TotalMilliseconds = "+cur_time.TotalMilliseconds);
			int secs = (int)cur_time.TotalSeconds;
			int nsecs = (int)((cur_time.TotalMilliseconds - secs*1000.0)*1000000);
			//OptarLogger.info("secs = "+secs+"  nsecs = "+nsecs+" msec diff = "+(cur_time.TotalMilliseconds - secs*1000.0));
			//OptarLogger.info("Setting up header with time: "+secs+", "+nsecs);
			seq++;
			stamp.secs = secs;
			stamp.nsecs = nsecs;
		}

		public long getTimeNano()
		{
			return Utils.toTimeNano(stamp);
		}
		public long getTimeMillis()
		{
			return Utils.toTimeMillis(stamp);
		}
	}
}