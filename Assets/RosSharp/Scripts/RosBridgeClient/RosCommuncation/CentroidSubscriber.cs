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
    public class CentroidSubscriber : Subscriber<Messages.OPT.TrackArray>
    {
        private Messages.OPT.TrackArray trackArray;
        private bool isMessageReceived;
        public Dictionary<int, Vector3> processedTrackData = new Dictionary<int, Vector3>();

        public delegate void ReceiveTrackData();
        public static event ReceiveTrackData OnReceive;

        //for system health averages
        private float beginTime = 0f;
        private int count  = 0;
        public float centroidRate = 0f;

        protected override void Start()
        {
            base.Start();
        }

        private void Update()
        {

            if (isMessageReceived){
                MessageRate();
                ProcessMessage();
            }

        }

        protected override void ReceiveMessage(Messages.OPT.TrackArray message)
        {
            trackArray = message;
            isMessageReceived = true;
        }

        private void ProcessMessage()
        {
            processedTrackData.Clear();
            foreach(Messages.OPT.Track track in trackArray.tracks){
                Vector3 v = new Vector3(track.x, track.y, track.height);
                if(!processedTrackData.ContainsKey(track.id)){
                    //processedTrackData.Add(track.id, new Vector3(track.x, track.height, track.y));
                    processedTrackData.Add(track.id, RHtoLHTransform(v));
                }
                else{
                    //processedTrackData[track.id] = new Vector3(track.x, track.height, track.y);
                    processedTrackData[track.id] = RHtoLHTransform(v);
                }

            }

            OnReceive?.Invoke();
            isMessageReceived = false;
        }

        private void MessageRate(){
            if (Time.time - beginTime < 1f){
                count += 1;
            }
            else{
                centroidRate = count / (Time.time - beginTime);
                beginTime = Time.time;
                count = 0;
            } 
        }
    }
}