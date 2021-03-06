﻿/*
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

namespace RosSharp.RosBridgeClient
{
    [RequireComponent(typeof(RosConnector))]
    public abstract class Subscriber<T> : MonoBehaviour where T: Message
    {
        public string Topic;
        public float TimeStep;

        protected virtual void Start()
        {
            GetComponent<RosConnector>().RosSocket.Subscribe<T>(Topic, ReceiveMessage, (int)(TimeStep)); // the rate(in ms in between messages) at which to throttle the topics
        }

        protected abstract void ReceiveMessage(T message);

        public void SetTopic(string topic)
        {
            Topic = topic;
        }


        // ROS is right-handed, Unity is left-handed.  Each subscriber inherits this function to transform before visualization
        public Vector3 RHtoLHTransform(Vector3 rh){
        
            Vector3 lh = new Vector3(rh.y, rh.z, -rh.x);
            return lh;
        }

        public Quaternion RHtoLHTransform(Quaternion rh){
            Quaternion lh = new Quaternion(-rh.y, -rh.z, rh.x, rh.w);
            return lh;
        }
    }
}