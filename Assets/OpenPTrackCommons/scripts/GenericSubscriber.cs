using UnityEngine;

namespace OpenPTrack
{

    /// <author>
    /// Carlo Rizzardo
    /// </author>
    /// <summary>
    /// Generic subscriber for ROS topics.
    /// 
    /// You will have to construct the object providing it a RosConnector and the topic name, 
    /// after this you will have to add a callback with the addCallback method.
    /// The RosConnect wil have to be already running and be connected when you use it in the constructor
    /// </summary>
    /// <typeparam name="T">The message typewe will be receiving</typeparam>
    public class GenericSubscriber<T> where T: RosSharp.RosBridgeClient.Message
    {
        public int throttlePeriod_millis = 1;
		public string topicName;

		public class OnMessageReceivedEvent : UnityEngine.Events.UnityEvent<T>
		{
			private int listenersCount = 0;
			public int getListenersCount()
			{
				return listenersCount;
			}

			public new void AddListener(UnityEngine.Events.UnityAction<T> listener)
			{
				listenersCount++;
				base.AddListener(listener);
			}
			
			public new void RemoveListener(UnityEngine.Events.UnityAction<T> listener)
			{
				base.RemoveListener(listener);
				listenersCount--;
			}
		};

		public OnMessageReceivedEvent onMessageReceived = new OnMessageReceivedEvent();
		public RosSharp.RosBridgeClient.RosConnector rosConnector;

        public GenericSubscriber(RosSharp.RosBridgeClient.RosConnector rosConnector, string topicName)
        {
			OptLogger.info("Subscribing to "+topicName);
			this.topicName = topicName;
            string id = rosConnector.RosSocket.Subscribe<T>(topicName, ReceiveMessage, throttlePeriod_millis); // the rate(in ms in between messages) at which to throttle the topics
			OptLogger.info("Subscribed to "+topicName+" id = "+id);
        }

        protected void ReceiveMessage(T message)
		{
			//OptarLogger.info("received message on topic "+topicName+", invoking "+onMessageReceived.getListenersCount()+" callbacks");
			onMessageReceived.Invoke(message);
			//OptarLogger.info("callbacks ran successfully");
		}

		public void addCallback(UnityEngine.Events.UnityAction<T> callback)
		{
			onMessageReceived.AddListener(callback);
			OptLogger.info("added callback for topic "+topicName);
		}
    }
}