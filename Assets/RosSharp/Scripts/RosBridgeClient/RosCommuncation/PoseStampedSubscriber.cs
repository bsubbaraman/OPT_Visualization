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
        public GameObject ARCoreWorldFiltered;

        //for system health averages
        private float beginTime = 0f;
        private int count = 0;
        public float mobileRate = 0f;

        public bool showPhones = false;  // bool to show the phone position.  value change from gui control script on button click
        protected override void Start()
        {
			base.Start();
		}
		
        private void Update()
        {
            if (showPhones)
            {
                if (isMessageReceived)
                {
                    ProcessMessage();
                    MessageRate();
                }
            }
        }

        protected override void ReceiveMessage(Messages.Geometry.PoseStamped message)
        {
            //Debug.Log(message.header.frame_id);
            //position = new Vector3(message.pose.position.x, -message.pose.position.y, message.pose.position.z);
            position = new Vector3(message.pose.position.z, -message.pose.position.y, -message.pose.position.x);
            //rotation = new Quaternion(message.pose.orientation.x, message.pose.orientation.y, message.pose.orientation.z, message.pose.orientation.w);
            rotation = new Quaternion(message.pose.orientation.z, message.pose.orientation.y, message.pose.orientation.x, message.pose.orientation.w);

            rotation *= Quaternion.Euler(0f, 90f, 0f); //trying to rotate cell phone model correctly
            isMessageReceived = true;
        }

        private void ProcessMessage()
        {
            if (!phoneInScene)
            {
                GameObject newPhone = Instantiate(MobilePhonePrefab, position, rotation, ARCoreWorldFiltered.transform);
                PublishedTransforms.Add(0, newPhone.transform);
                phoneInScene = true;
            }

            PublishedTransforms[0].position = position;
            PublishedTransforms[0].rotation = rotation;

            isMessageReceived = false;
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

        private void MessageRate()
        {
            if (Time.time - beginTime < 1f)
            {
                count += 1;
            }
            else
            {
                mobileRate = count / (Time.time - beginTime);
                beginTime = Time.time;
                count = 0;
            }
        }
    }

}