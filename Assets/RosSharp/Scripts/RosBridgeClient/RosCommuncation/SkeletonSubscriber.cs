/*
© Siemens AG, 2017-2018
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

using UnityEngine;
using System.Collections.Generic;
namespace RosSharp.RosBridgeClient
{
    public class SkeletonSubscriber : Subscriber<Messages.OPT.SkeletonTrackArray>
    {
        private Messages.OPT.SkeletonTrackArray trackArray;
        private bool isMessageReceived;
        //public Dictionary<int, Vector3> processedTrackData = new Dictionary<int, Vector3>();
        public Dictionary<int, Vector3[]> jointsData = new Dictionary<int, Vector3[]>();
        public Dictionary<int, Vector3> centroidPose = new Dictionary<int, Vector3>();

        public delegate void ReceiveTrackData();
        public static event ReceiveTrackData OnReceive;


        bool checkTime = false;
        public float ros_rcv_time;
        public Visualization viz;

        //for system health averages
        private float beginTime = 0f;
        private int count = 0;
        public float skeletonRate = 0f;

        //// Test to store previous joint position, trying to interpolate bw:
        //public Dictionary<int, Vector3[]> previousJointsData = new Dictionary<int, Vector3[]>();
        //public Dictionary<int, Vector3[]> previousJointsDataTemp = new Dictionary<int, Vector3[]>();

        protected override void Start()
        {
            base.Start();
        }

        private void Update()
        {
            if (isMessageReceived)
            {
                MessageRate();
                ProcessMessage();
            }

        }

        protected override void ReceiveMessage(Messages.OPT.SkeletonTrackArray message)
        {
            trackArray = message;
            isMessageReceived = true;
            checkTime = true;
        }

        private void ProcessMessage()
        {
            // ***** interpolate test
            //previousJointsData = jointsData;
            ros_rcv_time = Time.time;
            // *****
            jointsData.Clear();
            foreach(Messages.OPT.SkeletonTrack track in trackArray.tracks){
                //Debug.Log("123 - ROS: )
                bool infinityFound = false;

                Vector3[] jointDetected = new Vector3[track.joints.Length];
                int jointPositionFree = 0;
                foreach (Messages.OPT.Track3D track3D in track.joints)
                {
                    if (float.IsInfinity(System.Math.Abs(track3D.x)))
                        infinityFound = true;
                    //jointDetected[jointPositionFree++] = new Vector3(track3D.x, track3D.z, track3D.y);
                    jointDetected[jointPositionFree++] = new Vector3(track3D.y, track3D.z, -track3D.x);


                    if (jointPositionFree > 1 && (Mathf.Approximately(Vector3.Distance(jointDetected[jointPositionFree - 2], jointDetected[jointPositionFree - 1]), 0.0f) || Vector3.Distance(jointDetected[jointPositionFree - 2], jointDetected[jointPositionFree - 1]) > 2.5f))
                        infinityFound = true;
                }

                if (!infinityFound)
                {
                    Vector3 v = new Vector3(track.x, track.y, track.height);
                    Vector3 chest = new Vector3(jointDetected[14].x, jointDetected[14].y, jointDetected[14].x);
                    if (chest.z < -5f || chest.z > 5f || chest.x < -6f || chest.x > 2f){
                        Debug.Log("skeleton not in active region");
                        return;
                    }
                        if (!centroidPose.ContainsKey(track.id))
                    {
                        centroidPose.Add(track.id, RHtoLHTransform(v));
                        //centroidPose.Add(track.id, new Vector3(track.y, track.height, -track.x));
                    }
                    else
                    {
                        centroidPose[track.id] = RHtoLHTransform(v);
                        //centroidPose.Add(track.id, new Vector3(track.y, track.height, -track.x));
                    }

                    if (!jointsData.ContainsKey(track.id))
                    {
                        jointsData.Add(track.id, jointDetected);
                    }
                    else
                    {
                        jointsData[track.id] = jointDetected;
                    }
                }
                else
                {
                    Debug.Log("123 - ROS: Skipped track   # " + track.id);
                }
            }

            OnReceive?.Invoke();
            isMessageReceived = false;
        }

        private void MessageRate()
        {
            if (Time.time - beginTime < 1f)
            {
                count += 1;
            }
            else
            {
                skeletonRate = count / (Time.time - beginTime);
                beginTime = Time.time;
                count = 0;
            }
        }
    }

}