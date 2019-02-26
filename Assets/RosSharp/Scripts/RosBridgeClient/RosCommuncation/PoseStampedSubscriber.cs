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
    public class PoseStampedSubscriber : Subscriber<Messages.Geometry.PoseStamped>
    {
        public Dictionary<int, Transform> PublishedTransforms = new Dictionary<int, Transform>(); //for when multiple phones are possible
        private Vector3 position;
        private Quaternion rotation;
        private bool isMessageReceived;

        public GameObject MobilePhonePrefab;
        public bool phoneInScene = false; // for now, there is only capability for 1 phone in optar mobile 

        protected override void Start()
        {
			base.Start();
		}
		
        private void Update()
        {
            if (isMessageReceived)
                ProcessMessage();

            if (!phoneInScene){
                GameObject newPhone = Instantiate(MobilePhonePrefab, position, rotation);
                PublishedTransforms.Add(0, newPhone.transform);
                phoneInScene = true;
            }

            PublishedTransforms[0].position = position;
            PublishedTransforms[0].rotation = rotation;

        }

        protected override void ReceiveMessage(Messages.Geometry.PoseStamped message)
        {
            position = new Vector3(message.pose.position.x, message.pose.position.y, message.pose.position.z);
            rotation = new Quaternion(message.pose.orientation.x, message.pose.orientation.y, message.pose.orientation.z, message.pose.orientation.w);
            isMessageReceived = true;
        }

        private void ProcessMessage()
        {
            //PublishedTransform.position = position;
            //PublishedTransform.rotation = rotation;

            //Debug.Log(PublishedTransform.position);
            //Debug.Log(PublishedTransform.rotation);

            //if (!phoneInScene){
            //    Instantiate(MobilePhonePrefab, GetPosition(), PublishedTransform.rotation);
            //    phoneInScene = true;
            //}
        }

        public Vector3 GetPosition(Messages.Geometry.PoseStamped message)
        {
            return new Vector3(
                message.pose.position.x,
                message.pose.position.y,
                message.pose.position.z);
        }

        public float GetYCoordinates()
        {
            //2 fot the ROS -> ARCore mapping coordiantes
            return position[2];
        }

        public Quaternion GetRotation(Messages.Geometry.PoseStamped message)
        {
            return new Quaternion(
                message.pose.orientation.x,
                message.pose.orientation.y,
                message.pose.orientation.z,
                message.pose.orientation.w);
        }
    }
}