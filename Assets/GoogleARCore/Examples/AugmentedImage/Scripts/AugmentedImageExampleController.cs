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
    using System.Runtime.InteropServices;
    using GoogleARCore;
    using UnityEngine;
    using UnityEngine.UI;
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


        /// <summary>
        /// The position and rotation of camera.
        /// </summary>
        Vector3 positionCameraOffset;
        Vector3 positionCameraOffsetPrevious;

        Vector3 positionCentreImage;
        Quaternion rotationCentreImage;


        int counterErrorDistance = 0;
        bool controlDistance = false;

        private ros_lib.ROSCommunicationPersonal ros;


        private void Start()
        {
            PrintDebugMessage("D: -------- New execution --------");
            ros = new ros_lib.ROSCommunicationPersonal();
            ros.SetIpAddress(inputText);
        }

        public string inputText = "192.168.x.x";
        private TouchScreenKeyboard keyboard;

        public GUIStyle style;

        // Updates button's text while user is typing
        void OnGUI()
        {
            style.fontSize = 50;

            if (GUI.Button(new Rect(30, 20, 250, 90), inputText, style))
            {
                keyboard = TouchScreenKeyboard.Open(inputText, TouchScreenKeyboardType.Default);
            }

            if (keyboard != null)
            {
                inputText = keyboard.text;
            }

            if (!keyboard.active)
            {
                keyboard = null;
                ros.SetIpAddress(inputText);
                ros.TearDown();
            }

        }

        private void OnApplicationQuit()
        {
            ros.TearDown();
        }

        private void OnApplicationPause()
        {
            ros.TearDown();
        }


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


            GameObject arMarker = Instantiate(emptyGameObject, positionCentreImage, rotationCentreImage);
            
            GameObject cameraObject = Instantiate(emptyGameObject, FirstPersonCamera.transform.position, FirstPersonCamera.transform.rotation);
            cameraObject.transform.SetParent(arMarker.transform);

            ros.PublicationVodom(SwapCoordinates(cameraObject.transform.localPosition, cameraObject.transform.localRotation));

            PrintDebugMessage("I: Camera local  -> Position: " + cameraObject.transform.localPosition.ToString() + " * Quaternion: " + cameraObject.transform.localPosition.ToString());

            PrintDebugMessage("I: Update complete correctly!");
        }

        /// <summary>
        /// Losts the position, reset all.
        /// </summary>
        private void LostPosition(string message)
        {
            PrintDebugMessage("E: Position Lost! " + message);
            Destroy(anchorOrigin);
            Destroy(anchorOriginObject);
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


        /// <summary>
        /// Positions the camera lost.
        /// </summary>
        /// <returns><c>true</c>, if camera lost was positioned, <c>false</c> otherwise.</returns>
        private bool PositionCameraLost()
        {

            float distance = Vector3.Distance(positionCameraOffset, positionCameraOffsetPrevious);
            //Debug.Log("123 - Difference between positions: " + distance);

            if (distance > 0.7 && controlDistance)
            {
                PrintDebugMessage("E: Distance control with value: " + distance);
                return true;
            }
            else
            {
                positionCameraOffsetPrevious = positionCameraOffset;
            }

            return false;

        }

        ///// <summary>
        ///// Analizes the pose message arrived from ROS
        ///// </summary>
        ///// <param name="input_tag_message">Input tag message.</param>
        //private void AnalizeTagPoseMessage(geom_msgs.PoseStamped input_tag_message)
        //{
        
        //    //pose_real_tag = new Vector3(input_tag_message.pose.position.x, input_tag_message.pose.position.y, input_tag_message.pose.position.z);
        //    //quat_real_tag = new Quaternion(input_tag_message.pose.orientation.x, input_tag_message.pose.orientation.y, input_tag_message.pose.orientation.z, input_tag_message.pose.orientation.w);

        //    //Vector3 angle = quat_real_tag.eulerAngles;
        //    //Vector3 angle_reverse = new Vector3();


        //    //angle_reverse[0] = angle[0];
        //    //angle_reverse[1] = angle[2];
        //    //angle_reverse[2] = angle[1];

        //    //quat_real_tag = Quaternion.Euler(angle_reverse);

        //    //PrintDebugMessage("I: After pose from ROS: " + pose_real_tag + " r: " + quat_real_tag);
        //}


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