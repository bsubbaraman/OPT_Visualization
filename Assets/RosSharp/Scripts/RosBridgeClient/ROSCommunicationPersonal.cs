using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using System.Threading;
using RosSharp.RosBridgeClient;
using geom_msgs = RosSharp.RosBridgeClient.Messages.Geometry;
using std_msgs = RosSharp.RosBridgeClient.Messages.Standard;
using std_srvs = RosSharp.RosBridgeClient.Services.Standard;
using rosapi = RosSharp.RosBridgeClient.Services.RosApi;

namespace RosSharp.ARCore
{
    public class ROSCommunicationPersonal
    {

        private string ipAddress = "192.168.100.101";
        private string port = "9090";
        private ManualResetEvent isConnected = new ManualResetEvent(false);

        private static RosSocket rosSocket;

        private bool connectionCreated = false;

        public int Timeout = 10;

        private string id_sub_tag;
        private string id_image_pub;
        private string id_vodom_pub;

        private geom_msgs.PoseStamped tag_pose;
        private geom_msgs.PoseStamped origin_tag_pose;


        /// <summary>
        /// Setup this instance.
        /// </summary>
        private void Setup()
        {
            rosSocket = new RosSocket(new RosBridgeClient.Protocols.WebSocketNetProtocol("ws://" + ipAddress + ":" + port));

            if(rosSocket != null)
            {
                PrintDebugMessage("I: Socket created succesfully on: " + ipAddress);
                connectionCreated = true;
            }
            else
            {
                PrintDebugMessage("E: Error creating socket on: " + ipAddress);
            }
        }


        /// <summary>
        /// Sets the ip address.
        /// </summary>
        /// <param name="inputAddress">Input IP address.</param>
        public void SetIpAddress(string inputAddress)
        {
            ipAddress = inputAddress;
            PrintDebugMessage("I: Set new IP address: " + ipAddress);
        }


        /// <summary>
        /// Tears down.
        /// </summary>
        public void TearDown()
        {
            rosSocket.UnadvertiseService(id_sub_tag);
            rosSocket.UnadvertiseService(id_image_pub);
            rosSocket.UnadvertiseService(id_vodom_pub);

            rosSocket.Close();
            connectionCreated = false;
            PrintDebugMessage("W: Socket destroy!");
        }


        /// <summary>
        /// Checks the socket.
        /// </summary>
        private void CheckSocket()
        {
            if (!connectionCreated)
            {
                Setup();
                connectionCreated = true;
            }
        }



        /// <summary>
        /// Publications the vodom.
        /// </summary>
        /// <param name="tupleInput">(vector, quaternion).</param>
        public void PublicationVodom(Tuple<Vector3, Quaternion> tupleInput)
        {
            CheckSocket();

            id_vodom_pub = rosSocket.Advertise<geom_msgs.PoseStamped>("/arcore/vodom");

            //PoseStampedPublisher poseStamped = new PoseStampedPublisher();

            //poseStamped.SetParameterPoseStampedMessage(tupleInput.Item1, tupleInput.Item2);
            
            //rosSocket.Publish(id_vodom_pub, poseStamped.GetPoseStampedObject());
        }


        /// <summary>
        /// Publications the image recognized
        /// </summary>
        /// <param name="tupleInput">(vector, quaternion).</param>
        public void PublicationImage(Tuple<Vector3, Quaternion> tupleInput)
        {
            CheckSocket();

            id_image_pub = rosSocket.Advertise<geom_msgs.PoseStamped>("/arcore/tag_arcore");
            //PoseStampedPublisher poseStamped = new PoseStampedPublisher();

            //poseStamped.SetParameterPoseStampedMessage(tupleInput.Item1, tupleInput.Item2);
            //rosSocket.Publish(id_image_pub, poseStamped.GetPoseStampedObject());
        }


        /// <summary>
        /// Subscriptions the tag pose.
        /// </summary>
        /// <returns>The tag pose.</returns>
        public geom_msgs.PoseStamped SubscriptionTagPose()
        {
            CheckSocket();
            PrintDebugMessage("I: ciao");
            id_sub_tag = rosSocket.Subscribe<geom_msgs.PoseStamped>("/arcore/origin", SubscriptionPoseStamped);
            //rosSocket.Unsubscribe(id_sub_tag);
            return tag_pose;
        }


        /// <summary>
        /// Subscriptions the pose stamped.
        /// </summary>
        /// <param name="inputMessage">Input message.</param>
        private void SubscriptionPoseStamped(geom_msgs.PoseStamped inputMessage)
        {
            PrintDebugMessage("I: " + inputMessage.pose.position.x);
            tag_pose = inputMessage;
        }







        //DEFAULT METHODS

        public void PublicationTest(string messageInput)
        {
            CheckSocket();
            string id = rosSocket.Advertise<std_msgs.String>("/publication_test");
            std_msgs.String message = new std_msgs.String(messageInput);
            rosSocket.Publish(id, message);
            rosSocket.Unadvertise(id);
        }


        public void ServiceCallTest()
        {
            CheckSocket();
            rosSocket.CallService<rosapi.GetParamRequest, rosapi.GetParamResponse>("/rosapi/get_param", ServiceCallHandler, new rosapi.GetParamRequest("/rosdistro", "default"));
        }

        public void ServiceResponseTest()
        {
            CheckSocket();
            string id = rosSocket.AdvertiseService<std_srvs.TriggerRequest, std_srvs.TriggerResponse>("/service_response_test", ServiceResponseHandler);
            rosSocket.UnadvertiseService(id);
        }

        private void SubscriptionHandler(std_msgs.String message)
        {
            PrintDebugMessage("I: Message received: " + message.data);
        }

        private void ServiceCallHandler(rosapi.GetParamResponse message)
        {
        }

        private bool ServiceResponseHandler(std_srvs.TriggerRequest arguments, out std_srvs.TriggerResponse result)
        {
            CheckSocket();
            result = new std_srvs.TriggerResponse(true, "service response message");
            return true;
        }

        /// <summary>
        /// Prints the debug message.
        /// </summary>
        /// <param name="message">Message.</param>
        private void PrintDebugMessage(string message)
        {
            Debug.Log("123 - ROS - " + message);
        }
    }
}