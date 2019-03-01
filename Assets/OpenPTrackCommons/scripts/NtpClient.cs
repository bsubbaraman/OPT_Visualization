using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenPTrack
{
    /// <summary>
    /// A simple NTP client. It uses the ros messaging infrastructure to determine the
    /// clock time difference between the device and an external ros node.
    /// With this information it can then provide timestamps that are roughly synchronized
    /// with the ros network.
    /// 
    /// Use it instantiating it as a MonoBehaviour and setting the topic name in the Inspector.
    /// You will need to provide a RosConnector in the rosConnector property.
    /// 
    /// After this you can access this from anywhere using the static "singleton" field of this class.
    /// 
    /// On the ROS side you will need a node that responds to the queries made by this object.
    /// 
    /// </summary>
    /// <author>
    /// Carlo Rizzardo
    /// </author>
    public class NtpClient : MonoBehaviour
	{
		public string topicName;
		public static NtpClient singleton;

		public RosSharp.RosBridgeClient.RosConnector rosConnector;

		public GenericPublisher<OptarNtpMessageMsg> ntpPublisher;
		public GenericSubscriber<OptarNtpMessageMsg> ntpSubscriber;
		private string lastRequestId;
		private System.DateTime lastRequestTimeMillis;

		public const int queueSize = 100;
		private System.Collections.Generic.Queue<long> timeDiffs = new System.Collections.Generic.Queue<long>(queueSize);

		private long estimatedTimeDiffMicro = 0;
		// Use this for initialization
		void Awake()
		{
            if (singleton != null)
            {
                GameObject.Destroy(singleton);
                OptLogger.error("You are instantiating more than one NtpClient! You shouldn't do it!");
            }

			singleton = this;

			DontDestroyOnLoad(this);
		}

		void Start()
		{
				lastRequestTimeMillis=System.DateTime.UtcNow;
				ntpPublisher = new GenericPublisher<OptarNtpMessageMsg>(rosConnector,topicName);
				ntpPublisher.Advertise();
				ntpSubscriber = new GenericSubscriber<OptarNtpMessageMsg>(rosConnector,topicName);
				ntpSubscriber.addCallback(onMessageReceived);
		}

		public long getEstimatedTimeDiffMicro()
		{
			return estimatedTimeDiffMicro;
		}
		
		// Update is called once per frame
		void Update ()
		{
			double elapsedTime = (System.DateTime.UtcNow - lastRequestTimeMillis).TotalMilliseconds;
			//OptarLogger.info("elapsed time = "+elapsedTime);
			if((lastRequestId==null && elapsedTime > 1000) || elapsedTime>10000)
			{
				OptLogger.info("sending ntp query");
				lastRequestTimeMillis=System.DateTime.UtcNow;
				OptarNtpMessageMsg ntpRequest = new OptarNtpMessageMsg(OptarNtpMessageMsg.QUERY);
				lastRequestId=ntpRequest.id;
				ntpPublisher.Publish(ntpRequest);
				OptLogger.info("sent ntp query");
			}
			else
			{
				//OptarLogger.info("skipped ntp query request");
			}
		}

		private void onMessageReceived(OptarNtpMessageMsg message)
		{
			//OptarLogger.info("received ntp message");
			if(message.id.Equals(lastRequestId) && message.type==OptarNtpMessageMsg.REPLY)
			{
				//OptarLogger.info("received ntp response to our query");
				RosSharp.RosBridgeClient.Messages.Standard.Time clientReceiveTime = Utils.getRosTimeNow();
				long clientReceiveTimeMicro = Utils.rosTimeToMicro(clientReceiveTime);
				long clientRequestTimeMicro = Utils.rosTimeToMicro(message.clientRequestTime);
				long serverTimeMicro = Utils.rosTimeToMicro(message.serverTime);

				long timeDiffMicro = ((serverTimeMicro - clientRequestTimeMicro) + (serverTimeMicro - clientReceiveTimeMicro))/2;

				if(timeDiffs.Count>=queueSize)
					timeDiffs.Dequeue();
				timeDiffs.Enqueue(timeDiffMicro);
				long timeDiffsSum = 0;
				foreach(long td in timeDiffs)
				{
					timeDiffsSum+=td;
				}
				estimatedTimeDiffMicro = -timeDiffsSum/timeDiffs.Count;
				OptLogger.info("ntp estimated time difference = "+estimatedTimeDiffMicro+" us");
				lastRequestId = null;
			}
			else
			{
				//OptarLogger.info("ntp message discarded");
			}
		}

        /// <summary>
        /// Provide a synchronized timestamp
        /// </summary>
        /// <returns>
        /// The timestamp, i.e. milliseconds from 1 Jan 1970
        /// </returns>
        public long currentTimeMillis()
        {
            System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            System.DateTime now = System.DateTime.UtcNow;
            now = now + (new System.TimeSpan(0, 0, 0, 0, (int)(NtpClient.singleton.getEstimatedTimeDiffMicro() / 1000)));
            //OptarLogger.info("time correction = "+(new System.TimeSpan(0,0,0,0,(int)(NtpClient.singleton.getEstimatedTimeDiffMicro()/1000))));
            System.TimeSpan cur_time = now - epochStart;

            return (long)cur_time.TotalMilliseconds;
        }

        /// <summary>
        /// Provide a synchronized timestamp
        /// </summary>
        /// <returns>
        /// The timestamp, i.e. seconds from 1 Jan 1970
        /// </returns>
        public double currentTimeSecs()
        {
            System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            System.DateTime now = System.DateTime.UtcNow;
            now = now + (new System.TimeSpan(0, 0, 0, 0, (int)(NtpClient.singleton.getEstimatedTimeDiffMicro() / 1000)));
            //OptarLogger.info("time correction = "+(new System.TimeSpan(0,0,0,0,(int)(NtpClient.singleton.getEstimatedTimeDiffMicro()/1000))));
            System.TimeSpan cur_time = now - epochStart;

            return cur_time.TotalSeconds;
        }
    }
}