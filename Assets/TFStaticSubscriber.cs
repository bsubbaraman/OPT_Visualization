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
    public class TFStaticSubscriber : Subscriber<Messages.OPT.TFMessage>
    {
        private Messages.OPT.TFMessage tf;

        //public GameObject cam;
        private bool isMessageReceived;

        public GameObject ARCoreWorldFiltered;

        protected override void Start()
        {
            base.Start();
        }



        private void Update()
        {
            if (isMessageReceived)
                ProcessMessage();
        }

        protected override void ReceiveMessage(Messages.OPT.TFMessage message)
        {
            tf = message;
            isMessageReceived = true;
        }

        private void ProcessMessage()
        {
            string sensorname = tf.transforms[0].child_frame_id;
            //foreach (var child in tf.transforms)
            //{
            //    Debug.Log(child.child_frame_id);
            //}
            //Debug.Log(""+tf.transforms[2].transform.translation.x + ", " + tf.transforms[2].transform.translation.y + ", " + tf.transforms[2].transform.translation.z);
            //if(tf.transforms[0].child_frame_id == "/kinect01"){

            foreach (var child in tf.transforms)
            {
                if (child.child_frame_id == "arcore_world_filtered"){
                    Debug.Log("filtered");
                    Vector3 v = new Vector3(child.transform.translation.x, child.transform.translation.y, child.transform.translation.z);
                    Quaternion q = new Quaternion(child.transform.rotation.x, child.transform.rotation.y, child.transform.rotation.z, child.transform.rotation.w);

                    ARCoreWorldFiltered.transform.position = RHtoLHTransform(v);
                    ARCoreWorldFiltered.transform.rotation = RHtoLHTransform(q);

                }
            }
            isMessageReceived = false;

            //ARCoreWorldFiltered.transform.position = new Vector3(tf.transforms[2].transform.translation.x, tf.transforms[2].transform.translation.y, tf.transforms[2].transform.translation.z);
            //ARCoreWorldFiltered.transform.rotation = new Quaternion(tf.transforms[2].transform.rotation.x, tf.transforms[2].transform.rotation.y, tf.transforms[2].transform.rotation.z, tf.transforms[2].transform.rotation.w);

        }
    }
}