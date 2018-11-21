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

        protected override void Start()
        {
            base.Start();
        }

        private void Update()
        {
            if (isMessageReceived)
                ProcessMessage();
        }

        protected override void ReceiveMessage(Messages.OPT.SkeletonTrackArray message)
        {
            trackArray = message;
            isMessageReceived = true;
        }

        private void ProcessMessage()
        {
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
                    jointDetected[jointPositionFree++] = new Vector3(track3D.x, track3D.z, track3D.y);

                    if (jointPositionFree > 1 && (Mathf.Approximately(Vector3.Distance(jointDetected[jointPositionFree - 2], jointDetected[jointPositionFree - 1]), 0.0f) || Vector3.Distance(jointDetected[jointPositionFree - 2], jointDetected[jointPositionFree - 1]) > 1.0f))
                        infinityFound = true;
                }

                if (!infinityFound)
                {
                    if (!centroidPose.ContainsKey(track.id))
                    {
                        centroidPose.Add(track.id, new Vector3(track.x, track.height, track.y));
                    }
                    else
                    {
                        centroidPose[track.id] = new Vector3(track.x, track.height, track.y);
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
        }
    }
}