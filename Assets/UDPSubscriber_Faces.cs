using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using SimpleJSON;

namespace RosSharp.RosBridgeClient
{
    public class UDPSubscriber_Faces : MonoBehaviour
    {

        public UDPReceive receiver;
        private bool newData = false;
        public Dictionary<int, string> recognizedFaceData = new Dictionary<int, string>();
        void Start()
        {

        }

        void OnEnable()
        {
            UDPReceive.OnReceive += Data;
        }

        void OnDisable()
        {
            UDPReceive.OnReceive -= Data;
        }

        void Data()
        {
            newData = true;
        }

        void Update()
        {
            if (newData)
            {

                var data = receiver.getLatestUDPPacket();
                var N = JSON.Parse(data);
                if (N["people_tracks"] != null)
                {
                    recognizedFaceData.Clear();
                    for (int count = 0; count < N["people_tracks"].Count; count++)
                    {
                        int id = N["people_tracks"][count]["id"];
                        string face_name = N["people_tracks"][count]["face_name"];
                        if (!string.IsNullOrEmpty(face_name))
                        {
                            Debug.Log(face_name);
                            recognizedFaceData.Add(id, face_name);
                        }
                    }
                    newData = false;

                }
            }
        }
    }
}
