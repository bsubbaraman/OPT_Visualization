/*
© Siemens AG, 2017
Author: Dr. Martin Bischoff (martin.bischoff@siemens.com)

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

<http://www.apache.org/licenses/LICENSE-2.0>.

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Threading;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class RosConnector : MonoBehaviour
    {

        //
        public CentroidSubscriber centroidSubscriber;
        public SkeletonSubscriber skeletonSubscriber;
        public ObjectsSubscriber objectsSubscriber;
        public ImageSubscriber imageSubscriber;
        public FaceSubscriber faceSubscriber;
        public TFSubscriber tfSubscriber;
        //
        public int Timeout = 10;

        public RosSocket RosSocket { get; private set; }
        public enum Protocols { WebSocketSharp, WebSocketNET };
        public Protocols Protocol;
        public string RosBridgeServerUrl = "ws://192.168.0.1:9090";
        private bool connectionEstablished = false;
        private bool subscribersEnabled = false;
        private ManualResetEvent isConnected = new ManualResetEvent(false);
        //CHANGE IP

        private Rect windowRect = new Rect(Screen.width / 3, 2 * Screen.height / 5, Screen.width / 3, Screen.height / 5);
        public string IPEnter = "theIP:thePort";
        public GUISkin skin;
        public GUISkin defaultSkin;
        private bool setDefaultIP = false;

        void OnGUI()
        {
            skin.toggle = defaultSkin.toggle;
            GUI.skin = skin;
            if (!connectionEstablished && Time.realtimeSinceStartup > 10f)
            {
                windowRect = GUI.Window(0, windowRect, DoMyWindow, "Server Not Connected");
            }
        }

        // Make the contents of the window
        void DoMyWindow(int windowID)
        {
            GUI.Label(new Rect(20, 40, 50, 40), "ws://");
            IPEnter = GUI.TextField(new Rect(60, 40, windowRect.width / 2 - 40, 40), IPEnter);
            if (GUI.Button(new Rect(windowRect.width / 3 - 150, windowRect.height / 2 - 20, 150, 40), "Set New IP"))
            {
                print("Got a click");
                TearDown();
                SetAddress("ws://" + IPEnter);
                ConnectAndWait();
                Restart();
            }


            if (GUI.Toggle(new Rect(windowRect.width / 3 - 100, 3 * windowRect.height / 4 - 25, 100, 50), setDefaultIP, "Set Default?")){
                Debug.Log("Toggle Click");
                setDefaultIP = !setDefaultIP;
            }
            //if (GUI.Button(new Rect(windowRect.width / 3 - 100, 3*windowRect.height / 4 - 25, 100, 50), "Set Default?")){
            //    setDefaultIP = !setDefaultIP;
            //}
        }

        //END CHANGE IP
        //


        public void Awake()
        {
            //ConnectAndWait();
            //PrintDebugMessage("entro qua 0");
            new Thread(ConnectAndWait).Start();
        }

        public void ConnectAndWait()
        {
            RosSocket = ConnectToRos(Protocol, RosBridgeServerUrl, OnConnected, OnClosed);
            //PrintDebugMessage("entro qua");
            if (!isConnected.WaitOne(Timeout * 1000))
            {
                PrintDebugMessage("Failed to connect to RosBridge at: " + RosBridgeServerUrl);
                connectionEstablished = false;
            }
            else
            {
                connectionEstablished = true;
                //PrintDebugMessage("entro qua 2");
            }
        }

        void Update(){
            if (!subscribersEnabled){
                if (connectionEstablished)
                {
                    tfSubscriber.enabled = true;
                    centroidSubscriber.enabled = true;
                    objectsSubscriber.enabled = true;
                    skeletonSubscriber.enabled = true;
                    faceSubscriber.enabled = true;
                    imageSubscriber.enabled = true;
                    subscribersEnabled = false;
                }
            }
        }
           

        public bool ConnectionStatus()
        {
            return connectionEstablished;
        }
        
        public static RosSocket ConnectToRos(Protocols protocolType, string serverUrl, EventHandler onConnected = null, EventHandler onClosed = null)
        {
            RosBridgeClient.Protocols.IProtocol protocol = GetProtocol(protocolType, serverUrl);
            protocol.OnConnected += onConnected;
            protocol.OnClosed += onClosed;

            return new RosSocket(protocol);
        }

        private static RosBridgeClient.Protocols.IProtocol GetProtocol(Protocols protocol, string rosBridgeServerUrl)
        {
            switch (protocol)
            {
                case Protocols.WebSocketSharp:
                    return new RosBridgeClient.Protocols.WebSocketSharpProtocol(rosBridgeServerUrl);
                case Protocols.WebSocketNET:
                    return new RosBridgeClient.Protocols.WebSocketNetProtocol(rosBridgeServerUrl);
                default:
                    return null;
            }
        }

        public void TearDown()
        {
            connectionEstablished = false;
            isConnected.Reset();
            OnApplicationQuit();
        }

        private void OnApplicationQuit()
        {
            RosSocket.Close();
        }

        private void OnConnected(object sender, EventArgs e)
        {
            isConnected.Set();
            PrintDebugMessage("Connected to RosBridge: " + RosBridgeServerUrl);
        }

        private void OnClosed(object sender, EventArgs e)
        {
            isConnected.Reset();
            PrintDebugMessage("Disconnected from RosBridge: " + RosBridgeServerUrl);
        }

        public void SetAddress(string address)
        {
            RosBridgeServerUrl = address;
            PrintDebugMessage("New ip address set: " + RosBridgeServerUrl);
        }

        /// <summary>
        /// Prints the debug message.
        /// </summary>
        /// <param name="message">Message.</param>
        private void PrintDebugMessage(string message)
        {
            Debug.Log("123 - ROS " + message);
        }

        private void Restart(){
            centroidSubscriber.enabled = false;
            centroidSubscriber.enabled = true;

        }
    }
}