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
    public class FaceSubscriber : Subscriber<Messages.OPT.TrackArray>
    {
        private Messages.OPT.TrackArray trackArray;
        private bool isMessageReceived;
        public Dictionary<int, string> faceData = new Dictionary<int, string>();

        protected override void Start()
        {
            base.Start();
        }

        private void Update()
        {
            if (isMessageReceived)
                ProcessMessage();
        }

        protected override void ReceiveMessage(Messages.OPT.TrackArray message)
        {
            trackArray = message;
            isMessageReceived = true;
        }

        private void ProcessMessage()
        {
            faceData.Clear();
            foreach (Messages.OPT.Track track in trackArray.tracks)
            {
                string face_name = track.face_name;
                int id = track.id;
                if (!string.IsNullOrEmpty(face_name)){
                    if (!faceData.ContainsKey(track.id))
                    {
                        faceData.Add(track.id, face_name);
                    }
                }
               
                else
                {
                    // do i need this?  should just stay the same
                    faceData[track.id] = face_name;
                }

            }

        }
    }
}