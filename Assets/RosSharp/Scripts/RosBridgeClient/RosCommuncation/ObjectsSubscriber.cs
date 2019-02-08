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
    public class OPTObject{
        public Vector3 pos;
        public string objectID;
        public float age;
        public float confidence;
    }

    public class ObjectsSubscriber : Subscriber<Messages.OPT.TrackArray>
    {
        private Messages.OPT.TrackArray trackArray;
        private bool isMessageReceived;
        //public Dictionary<int, Vector3> objectTrackData = new Dictionary<int, Vector3>();
        public Dictionary<int, OPTObject> objectTrackData = new Dictionary<int, OPTObject>();

        public delegate void ReceiveTrackData();
        public static event ReceiveTrackData OnReceive;

        public float ros_rcv_time;

        //for system health averages
        private float beginTime = 0f;
        private int count = 0;
        public float objectRate = 0f;

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

        protected override void ReceiveMessage(Messages.OPT.TrackArray message)
        {
            trackArray = message;
            isMessageReceived = true;
        }

        private void ProcessMessage()
        {
            ros_rcv_time = Time.time;
            objectTrackData.Clear();
            foreach (Messages.OPT.Track track in trackArray.tracks)
            {
                Vector3 v = new Vector3(track.x, track.y, track.height);
                if (!objectTrackData.ContainsKey(track.id))
                {
                    OPTObject o = new OPTObject
                    {
                        //pos = new Vector3(track.x, track.height, track.y),
                        pos = RHtoLHTransform(v),
                        objectID = track.object_name,
                        age = track.age,
                        confidence = track.confidence

                    };
                    //objectTrackData.Add(track.id, new Vector3(track.x, track.height, track.y));
                    objectTrackData.Add(track.id, o);
                }
                else
                {
                    //objectTrackData[track.id].pos = new Vector3(track.x, track.height, track.y);
                    objectTrackData[track.id].pos = RHtoLHTransform(v);

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
                objectRate = count / (Time.time - beginTime);
                beginTime = Time.time;
                count = 0;
            }
        }

    }
}