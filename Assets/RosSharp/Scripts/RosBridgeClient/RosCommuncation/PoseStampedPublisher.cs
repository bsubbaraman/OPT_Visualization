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
using System;

namespace RosSharp.RosBridgeClient
{
    public class PoseStampedPublisher : Publisher<Messages.Geometry.PoseStamped>
    {
        private Transform PublishedTransform;
        private string FrameId = "";

        private Messages.Geometry.PoseStamped message;

        protected override void Start()
        {
            base.Start();
            InitializeMessage();
        }

        private void FixedUpdate()
        {
            UpdateMessage();
        }

        private void InitializeMessage()
        {
            message = new Messages.Geometry.PoseStamped
            {
                header = new Messages.Standard.Header()
                {
                    frame_id = FrameId
                }
            };
        }

        public void SendMessage(Tuple<Vector3, Quaternion> tupleInput)
        {
            UpdateMessage(tupleInput.Item1, tupleInput.Item2);
        }

        public void SendMessage(Tuple<Vector3, Quaternion> tupleInput, string name)
        {
            UpdateMessage(tupleInput.Item1, tupleInput.Item2, name);
        }

        private void UpdateMessage(Vector3 vectorInput, Quaternion quaternionInput, string name)
        {
            message.header.Update();
            message.header.frame_id = name;
            message.pose.position = GetGeometryPoint(vectorInput);
            message.pose.orientation = GetGeometryQuaternion(quaternionInput);

            Publish(message);
        }

        private void UpdateMessage(Vector3 vectorInput, Quaternion quaternionInput)
        {
            message.header.Update();
            message.pose.position = GetGeometryPoint(vectorInput);
            message.pose.orientation = GetGeometryQuaternion(quaternionInput);

            Publish(message);
        }

        private void UpdateMessage()
        {
            message.header.Update();
            message.header.frame_id = name;
            message.pose.position = GetGeometryPoint(PublishedTransform.position.Unity2Ros());
            message.pose.orientation = GetGeometryQuaternion(PublishedTransform.rotation.Unity2Ros());

            Publish(message);
        }

        private Messages.Geometry.Point GetGeometryPoint(Vector3 position)
        {
            Messages.Geometry.Point geometryPoint = new Messages.Geometry.Point();
            geometryPoint.x = position.x;
            geometryPoint.y = position.y;
            geometryPoint.z = position.z;
            return geometryPoint;
        }

        private Messages.Geometry.Quaternion GetGeometryQuaternion(Quaternion quaternion)
        {
            Messages.Geometry.Quaternion geometryQuaternion = new Messages.Geometry.Quaternion();
            geometryQuaternion.x = quaternion.x;
            geometryQuaternion.y = quaternion.y;
            geometryQuaternion.z = quaternion.z;
            geometryQuaternion.w = quaternion.w;
            return geometryQuaternion;
        }

    }
}
