//-----------------------------------------------------------------------
// <copyright file="AugmentedImageExampleController.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

using ros_lib = RosSharp.ARCore;
using geom_msgs = RosSharp.RosBridgeClient.Messages.Geometry;

namespace GoogleARCore.Examples.AugmentedImage
{
    using System.Collections.Generic;
    using GoogleARCore;
    using UnityEngine;
    using System;

    /// <summary>
    /// Controller for AugmentedImage example.
    /// </summary>
    public class AugmentedImageExampleController : MonoBehaviour
    {

        /// <summary>
        /// The first-person camera being used to render the passthrough camera image (i.e. AR background).
        /// </summary>
        public Camera FirstPersonCamera;

        /// <summary>
        /// A prefab for visualizing an AugmentedImage.
        /// </summary>
        public GameObject anchorObject;
        private GameObject anchorOriginObject;

        public GameObject emptyGameObject;
        public GameObject centroidObject;


        /// <summary>
        /// The overlay containing the fit to scan user guide.
        /// </summary>
        public GameObject FitToScanOverlay;

        private Dictionary<int, AugmentedImageVisualizer> m_Visualizers
            = new Dictionary<int, AugmentedImageVisualizer>();

        private List<AugmentedImage> m_TempAugmentedImages = new List<AugmentedImage>();

        /// <summary>
        /// The anchor origin.
        /// </summary>
        Anchor anchorOrigin = null;


        Vector3 positionCentreImage;
        Quaternion rotationCentreImage;

        int counterErrorDistance = 0;
        //bool controlDistance = false;

        public RosSharp.RosBridgeClient.CentroidSubscriber centroidSub;
        public RosSharp.RosBridgeClient.PoseStampedPublisher posePub;

        public Dictionary<int, GameObject> activeTracks = new Dictionary<int, GameObject>();
        private Dictionary<int, Color> colors = new Dictionary<int, Color>();
        private void Start()
        {
            PrintDebugMessage("D: -------- New execution --------");
        }

        private TouchScreenKeyboard keyboard;

        private Vector3 worldAbs;
        private Anchor worldAnchor;

        //public GUIStyle style;

        //// Updates button's text while user is typing
        //void OnGUI()
        //{
        //    style.fontSize = 50;

        //    if (GUI.Button(new Rect(30, 20, 250, 90), inputText, style))
        //    {
        //        keyboard = TouchScreenKeyboard.Open(inputText, TouchScreenKeyboardType.Default);
        //    }

        //    if (keyboard != null)
        //    {
        //        inputText = keyboard.text;
        //    }

        //    if (!keyboard.active)
        //    {
        //        keyboard = null;
        //        rosConnector.SetAddress("ws://" + inputText + ":9090");
        //    }

        //}


        /// <summary>
        /// The Unity Update method.
        /// </summary>
        public void Update()
        {
            // Exit the app when the 'back' button is pressed.
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }

            if (Session.Status != SessionStatus.Tracking && counterErrorDistance < 5)
            {
                counterErrorDistance++;
                PrintDebugMessage("W: Counter lost tracking #" + counterErrorDistance);
                return;
            }

            //Search anchor
            if (anchorOrigin == null)
            {
                FitToScanOverlay.SetActive(true);
                SearchAnchorOrigin();
                return;
            }

            //control if the tracking is lost for at least 5 times
            if (counterErrorDistance > 4)
            {
                LostPosition("Tracking problem");
                counterErrorDistance = 0;
                return;
            }
            counterErrorDistance = 0;

            //Control if the position is lost
            //if (PositionCameraLost() && controlDistance)
            //{
            //    LostPosition("Distance problem");
            //    return;
            //}


            //if (!rosConnector.connectionEstablished){
            //    Destroy(rosConnector);
            //    PrintDebugMessage("ROS: E :  Server not running on IP: " + inputText);
            //    SetupRosConnection();
            //    return;
            //}


            GameObject arMarker = Instantiate(emptyGameObject, positionCentreImage, rotationCentreImage);
            //arMarker.name = "Marker";

            GameObject cameraObject = Instantiate(emptyGameObject, FirstPersonCamera.transform.position, FirstPersonCamera.transform.rotation);
            cameraObject.transform.SetParent(arMarker.transform);
            //PrintDebugMessage("I: Object local  -> Position: " + messageToSend.position.ToString() + " * Quaternion: " + messageToSend.rotation.ToString());
            //PrintDebugMessage("I: Camera local  -> Position: " + cameraObject.localPosition.ToString() + " * Quaternion: " + cameraObject.transform.localPosition.ToString());

            posePub.SendMessage(SwapCoordinates(cameraObject.transform.localPosition, cameraObject.transform.localRotation), "ar_mobile");


            var dataFromCentroidSub = centroidSub.processedTrackData;
            //PrintDebugMessage("I: Received data from CentroidSub length: " + dataFromCentroidSub.Count);

            foreach (KeyValuePair<int, Vector3> track in dataFromCentroidSub)
            {
                //int id = track.Key;
                //Vector3 poseInput = track.Value;

                //add any people who have joined the scene

                if (!activeTracks.ContainsKey(track.Key))
                {
                    Color color = new Color(
                          UnityEngine.Random.Range(0f, 1f),
                          UnityEngine.Random.Range(0f, 1f),
                          UnityEngine.Random.Range(0f, 1f),
                                            1);

                    GameObject newCentroid = Instantiate(centroidObject);

                    newCentroid.transform.SetParent(arMarker.transform);
                    newCentroid.transform.localPosition = track.Value;

                    //activeTracks[id].name = "centroid" + track.Key;
                    newCentroid.GetComponent<Renderer>().material.color = color;

                    activeTracks.Add(track.Key, newCentroid);

                    //activeTracks[id].transform.SetParent(arMarker.transform);
                    //activeTracks[id].transform.localPosition = track.Value;

                    //activeTracks[id].name = "centroid" + track.Key;
                    //activeTracks[id].GetComponent<Renderer>().material.color = color;

                    //PrintDebugMessage("I: Crete centroid -> Parent: " + activeTracks[id].transform.parent.name + " | Position: " + activeTracks[id].transform.localPosition.ToString() + " | Id: " + id);

                }
                else
                {
                    //activeTracks[id].SetActive(true);
                    activeTracks[track.Key].transform.localPosition = track.Value;
                    //PrintDebugMessage("I: Update centroid  -> Parent: " + activeTracks[id].transform.parent.name + " | Position: " + activeTracks[id].transform.localPosition.ToString() + " | Id: " + id);
                }

            }

            //remove any people who are no longer present
            foreach (KeyValuePair<int, GameObject> kvp in activeTracks)
            {
                if (!dataFromCentroidSub.ContainsKey(kvp.Key))
                {
                    activeTracks[kvp.Key].GetComponent<Renderer>().enabled = false;
                    //PrintDebugMessage("I: Remove centroid: " + kvp.Key);
                    activeTracks.Remove(kvp.Key);                
                }
            }

            //PrintDebugMessage("I: Update complete correctly!");
        }

        /// <summary>
        /// Losts the position, reset all.
        /// </summary>
        private void LostPosition(string message)
        {
            PrintDebugMessage("E: Position Lost! " + message);
            Destroy(anchorOrigin);
            Destroy(anchorOriginObject);
            foreach (KeyValuePair<int, GameObject> kvp in activeTracks)
            {
                Destroy(activeTracks[kvp.Key]);
                PrintDebugMessage("I: Destroy centroid: " + kvp.Key);
                activeTracks.Remove(kvp.Key);
            }
            counterErrorDistance = 0;
        }

        /// <summary>
        /// Prints the debug message.
        /// </summary>
        /// <param name="message">Message.</param>
        private void PrintDebugMessage(string message)
        {
            Debug.Log("123 - " + message);
        }

        /// <summary>
        /// Searchs the anchor origin.
        /// </summary>
        private void SearchAnchorOrigin()
        {
            // Get updated augmented images for this frame.
            Session.GetTrackables<AugmentedImage>(m_TempAugmentedImages, TrackableQueryFilter.Updated);

            // Create visualizers and anchors for updated augmented images that are tracking and do not previously
            // have a visualizer. Remove visualizers for stopped images.
            foreach (var image in m_TempAugmentedImages)
            {
                if (image.TrackingState == TrackingState.Tracking)
                {
                    // Create an anchor to ensure that ARCore keeps tracking this augmented image.
                    anchorOrigin = image.CreateAnchor(image.CenterPose);

                    positionCentreImage = image.CenterPose.position;
                    rotationCentreImage = image.CenterPose.rotation;

                    PrintDebugMessage("I: Position: P:" + image.CenterPose.position.ToString() + " , quat: " + image.CenterPose.rotation.ToString());

                    FitToScanOverlay.SetActive(false);

                    anchorOriginObject = Instantiate(anchorObject, image.CenterPose.position, image.CenterPose.rotation);
                    anchorOriginObject.transform.parent = anchorOrigin.transform;

                    PrintDebugMessage("I: Anchor created!");

                    // controlDistance = false;

                    PrintDebugMessage("I: " + image.ExtentX + " H: " + image.ExtentZ);


                }
            }
        }



        ///<summary>
        /// Positions the camera lost.
        /// </summary>
        /// <returns><c>true</c>, if camera lost was positioned, <c>false</c> otherwise.</returns>
        private bool PositionCameraLost()
        {

            ////float distance = Vector3.Distance(positionCameraOffset, positionCameraOffsetPrevious);
            ////Debug.Log("123 - Difference between positions: " + distance);

            //if (distance > 0.7 && controlDistance)
            //{
            //    PrintDebugMessage("E: Distance control with value: " + distance);
            //    return true;
            //}
            //else
            //{
            //    //positionCameraOffsetPrevious = positionCameraOffset;
            //}

            return false;

        }


        private Tuple<Vector3, Quaternion> SwapCoordinates(Vector3 poseInput, Quaternion quaternionInput)
        {

            Vector3 pose_swap = new Vector3(poseInput.x, poseInput.z, poseInput.y);

            Vector3 angle = quaternionInput.eulerAngles;
            Vector3 angle_swap = new Vector3();
            
            angle_swap[0] = angle[0];
            angle_swap[1] = angle[2];
            angle_swap[2] = angle[1];

            quaternionInput = Quaternion.Euler(angle_swap);

            return Tuple.Create<Vector3, Quaternion>(pose_swap, quaternionInput);
        }

    }

}