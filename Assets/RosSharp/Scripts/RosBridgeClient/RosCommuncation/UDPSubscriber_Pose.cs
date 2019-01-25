using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using SimpleJSON;

namespace RosSharp.RosBridgeClient
{
    public class RecognizedPose
    {
        public int id;
        public string pose_name;
        public float score;
    }
    public class UDPSubscriber_Pose : MonoBehaviour
    {

        public UDPReceive receiver;
        private bool newData = false;
        public Dictionary<int, RecognizedPose> recognizedPoseData = new Dictionary<int, RecognizedPose>();
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
                if (N["pose_tracks"] != null){
                    recognizedPoseData.Clear();
                    for (int count = 0; count < N["pose_tracks"].Count; count++){
                        RecognizedPose rP = new RecognizedPose();
                        rP.id = N["pose_tracks"][count]["id"];
                        rP.pose_name = N["pose_tracks"][count]["predicted_pose_name"];
                        rP.score = N["pose_tracks"][count]["predicted_score"];
                        recognizedPoseData.Add(rP.id, rP);
                    } 
                }
                newData = false;


            }
        }
    }
}
