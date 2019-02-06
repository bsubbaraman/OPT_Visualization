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
        //
        public int Timeout = 10;

        public RosSocket RosSocket { get; private set; }
        public enum Protocols { WebSocketSharp, WebSocketNET };
        public Protocols Protocol;
        public string RosBridgeServerUrl = "ws://192.168.0.1:9090";
        private bool connectionEstablished = false;

        //CHANGE IP
        //private Rect windowRect = new Rect(Screen.width / 2 - (Screen.width / 4 / 2), Screen.height / 2 - Screen.height / 4 / 2, Screen.width / 4, Screen.height / 4);
        //public string IPEnter = "theIP";

        //void OnGUI()
        //{
        //    if (!connectionEstablished)
        //    {
        //        windowRect = GUI.Window(0, windowRect, DoMyWindow, "Server Not Connected");
        //    }
        //}

        //// Make the contents of the window
        //void DoMyWindow(int windowID)
        //{
        //    IPEnter = GUI.TextField(new Rect(20, 40, Screen.width / 4 - 40, 20), IPEnter);
        //    if (GUI.Button(new Rect(windowRect.width / 2 - (Screen.width / 4 - 60) / 2, windowRect.height / 2 - 10, 150, 40), "Set New IP"))
        //    {
        //        print("Got a click");
        //        TearDown();
        //        SetAddress("ws://" + IPEnter + ":9090");
        //        ConnectAndWait();
        //        Restart();
        //    }
        //}

        //END CHANGE IP
        //
        private ManualResetEvent isConnected = new ManualResetEvent(false);

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