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
    public class PoseSubscriber : Subscriber<Messages.OPT.PoseRecognitionArray>
    {
        private Messages.OPT.PoseRecognitionArray poseArray;
        private bool isMessageReceived;
        public Dictionary<int, string> poseData = new Dictionary<int, string>();


        //for system health averages
        private float beginTime = 0f;
        private int count = 0;
        public float poseRate = 0f;

        protected override void Start()
        {
            base.Start();
        }

        private void Update()
        {
            if (isMessageReceived)
            {
                ProcessMessage();
                MessageRate();
            }
        }

        protected override void ReceiveMessage(Messages.OPT.PoseRecognitionArray message)
        {
            poseArray = message;
            isMessageReceived = true;
        }

        private void ProcessMessage()
        {
            poseData.Clear();
            foreach (Messages.OPT.PoseRecognition track in poseArray.poses)
            {
                int id = track.id;
                string pose_name = track.best_prediction_result.pose_name;
                if (track.best_prediction_result.pose_id != -1) //id = -1 when unknown
                {
                    if (!poseData.ContainsKey(id))
                    {
                        poseData.Add(id, pose_name);
                        Debug.Log(pose_name);
                    }
                }

                else
                {
                    // do i need this?  should just stay the same
                }

            }

        }

        private void MessageRate()
        {
            if (Time.time - beginTime < 1f)
            {
                count += 1;
            }
            else
            {
                poseRate = count / (Time.time - beginTime);
                beginTime = Time.time;
                count = 0;
            }
        }
    }
}