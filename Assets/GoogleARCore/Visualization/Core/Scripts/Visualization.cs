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


namespace GoogleARCore.Visualization.Core
{
    using System.Collections.Generic;
    using GoogleARCore;
    using UnityEngine;
    using System;
    using System.Threading;

    /// <summary>
    /// Controller for AugmentedImage example.
    /// </summary>
    public class Visualization : MonoBehaviour
    {

        /// <summary>
        /// The first-person camera being used to render the passthrough camera image (i.e. AR background).
        /// </summary>
        public Camera FirstPersonCamera;

        public GameObject anchorObject;
        public GameObject emptyGameObject;
        public GameObject centroidObject;
        private GameObject anchorOriginObject;

        public ParticleSystem partSystem;
        
        /// <summary>
        /// The overlay containing the fit to scan user guide.
        /// </summary>
        public GameObject FitToScanOverlay;
        public GameObject ServerConnection;

        public Dictionary<int, GameObject> activeTracks = new Dictionary<int, GameObject>();
        private Dictionary<int, VisualizationVisualizer> m_Visualizers = new Dictionary<int, VisualizationVisualizer>();
        private Dictionary<int, Color> colors = new Dictionary<int, Color>();
        private List<AugmentedImage> m_TempAugmentedImages = new List<AugmentedImage>();

        /// <summary>
        /// The anchor origin.
        /// </summary>
        private Anchor anchorOrigin = null;

        private Vector3 positionCentreImage;
        private Quaternion rotationCentreImage;

        private TouchScreenKeyboard keyboard;
        public GUIStyle style;
        public String ipAddress;

        int counterErrorDistance = 0;

        public RosSharp.RosBridgeClient.RosConnector rosConnector;
        public RosSharp.RosBridgeClient.CentroidSubscriber centroidSub;
        public RosSharp.RosBridgeClient.PoseStampedPublisher posePub;



        private void Start()
        {
            PrintDebugMessage("D: -------- New execution --------");
        }

        //// Updates button's text while user is typing
        //void OnGUI()
        //{
        //    style.fontSize = 50;

        //    if (GUI.Button(new Rect(0, 0, 350, 100), ipAddress, style))
        //    {
        //        keyboard = TouchScreenKeyboard.Open(ipAddress, TouchScreenKeyboardType.Default);
        //    }

        //    if (keyboard != null)
        //    {
        //        ipAddress = keyboard.text;
        //    }

        //    if (!keyboard.active)
        //    {
        //        keyboard = null;
        //        ConnectionToRos();
        //    }

        //}

        private void ConnectionToRos()
        {
            //Destroy(rosPrivate);
            rosConnector.TearDown();
            PrintDebugMessage("ROS - E :  Server not running on: " + rosConnector.RosBridgeServerUrl);
            rosConnector.SetAddress("ws://" + ipAddress + ":9090");
            rosConnector.Awake();
            //rosPrivate = Instantiate(ros);
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

            if (!rosConnector.ConnectionStatus())
            {
                ServerConnection.SetActive(true);
                //ConnectionToRos();
                return;
            }

            //Search anchor
            if (anchorOrigin == null)
            {
                ServerConnection.SetActive(false);
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


            //Send the pose to ROS
            PoseSender();
            //Take the data and create centroids
            CreateCentroidFromRosData();


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

                    //anchorOriginObject = Instantiate(anchorObject, image.CenterPose.position, image.CenterPose.rotation);
                    //anchorOriginObject.transform.parent = anchorOrigin.transform;

                    PrintDebugMessage("I: Anchor created!");
                    //PrintDebugMessage("I: " + image.ExtentX + " H: " + image.ExtentZ);
                }
            }
        }



        /// <summary>
        /// Swaps the coordinates from (xyz) to (xzy)
        /// </summary>
        /// <returns>Tuple with swaped coordinates.</returns>
        /// <param name="poseInput">Pose input.</param>
        /// <param name="quaternionInput">Quaternion input.</param>
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

        /// <summary>
        /// Send the pose of the camera to ROS.
        /// </summary>
        private void PoseSender()
        {
            GameObject cameraObject = Instantiate(emptyGameObject, FirstPersonCamera.transform.position, FirstPersonCamera.transform.rotation);
            cameraObject.transform.SetParent(anchorOrigin.transform);

            posePub.SendMessage(SwapCoordinates(cameraObject.transform.localPosition, cameraObject.transform.localRotation), SystemInfo.deviceUniqueIdentifier);
        }


        private void CreateParticleSystem(GameObject newCentroid, Color color)
        {
            ParticleSystem newParticular = Instantiate(partSystem);
            var mainPartSyst = newParticular.main;
            mainPartSyst.duration = 5.00f;
            mainPartSyst.startDelay = 0f;
            mainPartSyst.startLifetime = 1.4f;
            mainPartSyst.startSpeed = 0f;
            mainPartSyst.simulationSpace = ParticleSystemSimulationSpace.World;
            mainPartSyst.scalingMode = ParticleSystemScalingMode.Local;
            mainPartSyst.simulationSpeed = 1f;
            mainPartSyst.startRotation = 0f;
            mainPartSyst.flipRotation = 0f;
            mainPartSyst.startSize = 0.05f;
            mainPartSyst.startColor = color;

            var emissionPartSyst = newParticular.emission;
            emissionPartSyst.enabled = true;
            emissionPartSyst.rateOverTime = 0f;
            emissionPartSyst.rateOverDistance = 10f;

            var shapePartSyst = newParticular.shape;
            shapePartSyst.enabled = true;
            shapePartSyst.shapeType = ParticleSystemShapeType.Sphere;
            shapePartSyst.radius = 0.01f;
            shapePartSyst.radiusThickness = 1f;



            Gradient gradient = new Gradient();
            GradientColorKey[] colorKey;
            GradientAlphaKey[] alphaKey;

            // Populate the color keys at the relative time 0 and 1 (0 and 100%)
            colorKey = new GradientColorKey[2];
            colorKey[0].color = color;
            colorKey[0].time = 0.0f;
            colorKey[1].color = color;
            colorKey[1].time = 1.0f;

            // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
            alphaKey = new GradientAlphaKey[2];
            alphaKey[0].alpha = 1.0f;
            alphaKey[0].time = 0.782f;
            alphaKey[1].alpha = 0.0f;
            alphaKey[1].time = 1.0f;
            gradient.SetKeys(colorKey, alphaKey);

            var colorOverTimePartSyst = newParticular.colorOverLifetime;
            colorOverTimePartSyst.enabled = true;
            colorOverTimePartSyst.color = gradient;

            var sizeOverTimePartSyst = newParticular.sizeOverLifetime;
            sizeOverTimePartSyst.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0.0f, 1.0f);
            curve.AddKey(1.0f, 0.0f);
            sizeOverTimePartSyst.size = new ParticleSystem.MinMaxCurve(1.5f, curve);


            newParticular.transform.SetParent(newCentroid.transform);
            newParticular.transform.localPosition = Vector3.zero;
        }


        /// <summary>
        /// Creates the centroids from ros data.
        /// </summary>
        private void CreateCentroidFromRosData()
        {
            Dictionary<int, Vector3> dataFromCentroidSub = centroidSub.processedTrackData;
            PrintDebugMessage("I: Received data from CentroidSub length: " + dataFromCentroidSub.Count);

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

                    newCentroid.transform.SetParent(anchorOrigin.transform);
                    newCentroid.transform.localPosition = track.Value;
                    newCentroid.name = "centroid_" + track.Key;
                    newCentroid.GetComponent<Renderer>().material.color = color;

                    CreateParticleSystem(newCentroid, color);


                    //newCentroid.GetComponent<ChangeColorParticleSystem>().SetColor(color);

                    //ParticleSystem.MainModule settings = GetComponent<ParticleSystem>().main;
                    //settings.startColor = color;

                    activeTracks.Add(track.Key, newCentroid);

                    //PrintDebugMessage("I: Crete centroid -> Parent: " + activeTracks[id].transform.parent.name + " | Position: " + activeTracks[id].transform.localPosition.ToString() + " | Id: " + id);
                }
                else
                {
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
                    PrintDebugMessage("I: Remove centroid: " + kvp.Key);
                    activeTracks.Remove(kvp.Key);
                }
            }
        }
    }
}