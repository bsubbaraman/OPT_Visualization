/*
 
    -----------------------
    UDP-Receive (send to)
    -----------------------
    // [url]http://msdn.microsoft.com/de-de/library/bb979228.aspx#ID0E3BAC[/url]
   
   
    // > receive
    // 127.0.0.1 : 8051
   
    // send
    // nc -u 127.0.0.1 8051
 
*/
using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
namespace RosSharp.RosBridgeClient
{

        public class UDPReceive : MonoBehaviour
        {

            // receiving Thread
            Thread receiveThread;

            // udpclient object
            UdpClient client;

            //public string IP = "131.179.142.85";
            //public string IP = "192.168.100.255";
            public int port = 21234; // define > init

            // infos
            public string lastReceivedUDPPacket = "";
            public string allReceivedUDPPackets = ""; // clean up this from time to time!

            // Event to manage receiving new data
            public delegate void DataReceive();
            public static event DataReceive OnReceive;

            // start from shell
            private static void Main()
            {
                UDPReceive receiveObj = new UDPReceive();
                receiveObj.init();

                string text = "";
                do
                {
                    //text = Console.ReadLine();
                }
                while (!text.Equals("exit"));
            }

            public void Start()
            {
                init();
            }

            private void init()
            {
                print("UDPSend.init()");

                // define port
                port = 21234;

                // status
                print("Sending to 127.0.0.1 : " + port);
                print("Test-Sending to this Port: nc -u 127.0.0.1  " + port + "");


                // ----------------------------
                // Abhören
                // ----------------------------
                // Lokalen Endpunkt definieren (wo Nachrichten empfangen werden).
                // Einen neuen Thread für den Empfang eingehender Nachrichten erstellen.
                receiveThread = new Thread(
                    new ThreadStart(ReceiveData))
                {
                    IsBackground = true
                };
                receiveThread.Start();

            }

            // receive thread
            private void ReceiveData()
            {
                client = new UdpClient(port);
                while (true)
                {
                    try
                    {
                        // Bytes empfangen.
                        IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, port);
                        byte[] data = client.Receive(ref anyIP);
                        // Bytes mit der UTF8-Kodierung in das Textformat kodieren.
                        string text = Encoding.UTF8.GetString(data);
                        // latest UDPpacket
                        lastReceivedUDPPacket = text;

                        // ....
                        allReceivedUDPPackets = allReceivedUDPPackets + text;

                        if (OnReceive != null)
                        {
                            OnReceive();
                        }

                    }
                    catch (Exception err)
                    {
                        print(err.ToString());
                    }
                }
            }

            // getLatestUDPPacket
            // cleans up the rest
            public string getLatestUDPPacket()
            {
                allReceivedUDPPackets = "";
                return lastReceivedUDPPacket;
            }
        }
}