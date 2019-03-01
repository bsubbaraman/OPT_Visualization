using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenPTrack
{


    /// <author>
    /// Carlo Rizzardo
    /// </author>
    /// <summary>
    /// Generic publisher for ROS topics.
    /// 
    /// You will have to construct the object providing it a RosConnector and the topic name, and
    /// after this you must advertise the topic with the Advertise method.
    /// Then you will be able to use the Publish method.
    /// The RosConnect wil have to be already running and be connected when you use it in the constructor
    /// 
    /// </summary>
    /// <typeparam name="T">The message typewe will be publishing</typeparam>
    public class GenericPublisher<T> where T: RosSharp.RosBridgeClient.Message
	{

        private string topicName;
        private string publicationId;
        private RosSharp.RosBridgeClient.RosConnector rosConnector;

        public GenericPublisher(RosSharp.RosBridgeClient.RosConnector rosConnector, string topicName)
        {
            if(rosConnector==null)
                throw new System.ArgumentException("rosConnector cannot be null");
            if(topicName==null)
                throw new System.ArgumentException("topicName cannot be null");
            this.rosConnector = rosConnector;
            this.topicName = topicName;
        }

        public void Advertise()
        {
 			OptLogger.info("advertising topic "+topicName);
            publicationId = rosConnector.RosSocket.Advertise<T>(topicName);
            OptLogger.info("advertised topic "+topicName+", id = "+publicationId);
        }

        public void Publish(T message)
        {
            //Optar.OptarLogger.info("Publishing on topic "+topicName);
            if(rosConnector==null)
                throw new System.InvalidOperationException("Called publish with rosconnector not set");
            rosConnector.RosSocket.Publish(publicationId, message);
        }


	}
}