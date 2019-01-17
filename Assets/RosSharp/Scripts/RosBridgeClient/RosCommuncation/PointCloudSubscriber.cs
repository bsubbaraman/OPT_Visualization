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
    public class PointCloudSubscriber : Subscriber<Messages.Sensor.PointCloud2>
    {
        private Messages.Sensor.PointCloud2 pointCloud;
        private bool isMessageReceived;
        bool get = true;
        private const int k_MaxPointCount = 61440;

        private Mesh mesh;

        private Vector3[] m_Points = new Vector3[k_MaxPointCount];

        //public delegate void ReceivePointCloudData();
        //public static event ReceivePointCloudData OnReceive;

        protected override void Start()
        {
            base.Start();
            mesh = new Mesh();
            mesh.Clear();
        }

        private void Update()
        {
            if (isMessageReceived)
                ProcessMessage();
        }

        protected override void ReceiveMessage(Messages.Sensor.PointCloud2 message)
        {
            pointCloud = message;
            isMessageReceived = true;
        }

        private void ProcessMessage()
        {
            //Debug.Log(pointCloudData);
            //foreach (Messages.Sensor.PointField pF in pointCloud.fields){
            //    Debug.Log(pF.offset);
            //}
            //for (int i = 0; i < Frame.PointCloud.PointCount; i++)
            //{
            //    m_Points[i] = Frame.PointCloud.GetPoint(i);
            //}
        }
    }
}