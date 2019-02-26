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
    public class TFSubscriber : Subscriber<Messages.OPT.TFMessage>
    {
        private Messages.OPT.TFMessage tf;
        //public GameObject cam;
        private bool isMessageReceived;
        public Camera main;

        //for now, just manually add sensor names.  see if ros can publish sensor names to avoid needing gui selection
        public List<string> sensors = new List<string>();
        public Dictionary<string, bool> sensors_dict = new Dictionary<string, bool>();
        public GameObject PartsManager;
        public GameObject SensorPrefab;
        protected override void Start()
        {
            base.Start();

            foreach (string sensor_name in sensors)
            {
                sensors_dict.Add(sensor_name, false);
            }
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
            //if(tf.transforms[0].child_frame_id == "/kinect01"){
            if (sensors_dict.ContainsKey(sensorname))
            {
                if (!sensors_dict[sensorname]){
                    GameObject newSensor = Instantiate(SensorPrefab);
                    newSensor.name = sensorname.Remove(0,1);
                    newSensor.transform.parent = PartsManager.transform;
                    Vector3 v = new Vector3(tf.transforms[0].transform.translation.x, tf.transforms[0].transform.translation.y, tf.transforms[0].transform.translation.z);
                    Quaternion q = new Quaternion(tf.transforms[0].transform.rotation.x, tf.transforms[0].transform.rotation.y, tf.transforms[0].transform.rotation.z, tf.transforms[0].transform.rotation.w);
                    Debug.Log(q.eulerAngles);

                    Vector3 cameraPos = RHtoLHTransform(v);
                    Quaternion cameraRot = RHtoLHTransform(q);

                    newSensor.transform.position = cameraPos;
                    newSensor.transform.rotation = cameraRot;

                    newSensor.transform.GetChild(0).gameObject.GetComponent<LabelAlign>().main = main;
                    newSensor.transform.GetChild(0).gameObject.SetActive(true);
                    sensors_dict[sensorname] = true;
                }
                //cam.SetActive(true);
                //Vector3 v = new Vector3(tf.transforms[0].transform.translation.x, tf.transforms[0].transform.translation.y, tf.transforms[0].transform.translation.z);
                //Quaternion q = new Quaternion(tf.transforms[0].transform.rotation.x, tf.transforms[0].transform.rotation.y, tf.transforms[0].transform.rotation.z, tf.transforms[0].transform.rotation.w);
                //Debug.Log(v);
                //Debug.Log(q.eulerAngles);

                //cameraPos = RHtoLHTransform(v);
                //cameraRot = RHtoLHTransform(q);

                //cam.transform.position = cameraPos;
                //cam.transform.rotation = cameraRot;
            }

        }
    }
}