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

        public GameObject avatarPrefab;

        public ParticleSystem partSystem;
        
        /// <summary>
        /// The overlay containing the fit to scan user guide.
        /// </summary>
        public GameObject FitToScanOverlay;
        public GameObject ServerConnection;

        public Dictionary<int, GameObject> activeTracks = new Dictionary<int, GameObject>();
        public Dictionary<int, ParticleSystem> particles = new Dictionary<int, ParticleSystem>();
        public Dictionary<int, GameObject> activeSkeleton = new Dictionary<int, GameObject>();

        private Dictionary<int, VisualizationVisualizer> m_Visualizers = new Dictionary<int, VisualizationVisualizer>();
        private Dictionary<int, Color> colors = new Dictionary<int, Color>();
        private List<AugmentedImage> m_TempAugmentedImages = new List<AugmentedImage>();

        /// <summary>
        /// The anchor origin.
        /// </summary>
        private Anchor anchorOrigin = null;

        private Vector3 positionCentreImage;
        private Quaternion rotationCentreImage;

        //private TouchScreenKeyboard keyboard;
        //public GUIStyle style;
        //public String ipAddress;

        int counterErrorDistance = 0;

        public RosSharp.RosBridgeClient.RosConnector rosConnector;
        public RosSharp.RosBridgeClient.CentroidSubscriber centroidSub;
        public RosSharp.RosBridgeClient.SkeletonSubscriber skeletonSub;
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
            ////Destroy(rosPrivate);
            //rosConnector.TearDown();
            //PrintDebugMessage("ROS - E :  Server not running on: " + rosConnector.RosBridgeServerUrl);
            //rosConnector.SetAddress("ws://" + ipAddress + ":9090");
            //rosConnector.Awake();
            ////rosPrivate = Instantiate(ros);
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
            //PoseSender();
            GameObject cameraObject = Instantiate(emptyGameObject, FirstPersonCamera.transform.position, FirstPersonCamera.transform.rotation);
            cameraObject.transform.SetParent(anchorOrigin.transform);

            posePub.SendMessage(SwapCoordinates(cameraObject.transform.localPosition, cameraObject.transform.localRotation), SystemInfo.deviceUniqueIdentifier);
            //Take the data and create centroids
            //CreateCentroidFromRosData();

            Dictionary<int, Vector3> dataFromSkeletonSubCentroid = skeletonSub.centroidPose;
            Dictionary<int, Vector3[]> dataFromSkeletonSubSkeleton = skeletonSub.jointsData;
            //PrintDebugMessage("I: Received data from SkeletonSub length: " + dataFromSkeletonSubSkeleton.Count);


            foreach (KeyValuePair<int, Vector3[]> track in dataFromSkeletonSubSkeleton)
            {

                //PrintDebugMessage("I: distance: " + Vector3.Distance(positionCentroid, cameraObject.transform.localPosition));

                if (!activeSkeleton.ContainsKey(track.Key))
                {
                    GameObject newSkeleton = Instantiate(avatarPrefab);
                    //newSkeleton.transform.SetParent(anchorOrigin.transform);
                    //newSkeleton.name = "Skeleton_" + track.Key;

                    SetParentValue(newSkeleton);
                    activeSkeleton.Add(track.Key, newSkeleton);
                    //PrintDebugMessage("I: Crete skeleton Id # " + track.Key);

                }

                //activeSkeleton[track.Key].transform.localPosition = new Vector3(dataFromSkeletonSubCentroid[track.Key].x, dataFromSkeletonSubCentroid[track.Key].y, dataFromSkeletonSubCentroid[track.Key].z);

                SetJointsValue(activeSkeleton[track.Key], dataFromSkeletonSubSkeleton[track.Key]);
                //activeSkeleton[track.Key].transform.rotation = Quaternion.Euler(Quaternion.identity.eulerAngles[0], cameraObject.transform.localRotation.eulerAngles[1] + (float)Math.PI / 2, Quaternion.identity.eulerAngles[2]);

                //Avatar avatar = activeSkeleton[track.Key].GetComponent<>();

                //PrintDebugMessage("I:  " + 0 + " -> x: " + dataFromSkeletonSubSkeleton[track.Key][0].x + " _ y: " + dataFromSkeletonSubSkeleton[track.Key][0].y + " _ z: " + dataFromSkeletonSubSkeleton[track.Key][0].z);

                //PrintDebugMessage("I: Joints:");
                //int counterJoints = 0;
                //foreach (Vector3 joint in track.Value)
                //{
                //    PrintDebugMessage("I:  " + counterJoints++ + " -> x: " + joint.x + " _ y: " + joint.y + " _ z: " + joint.z);
                //}
            }
        

            //remove any people who are no longer present
            if (activeSkeleton.Count > 0)
            {
                foreach (KeyValuePair<int, GameObject> kvp in activeSkeleton)
                {
                    if (!dataFromSkeletonSubSkeleton.ContainsKey(kvp.Key))
                    {
                        activeSkeleton[kvp.Key].SetActive(false);
                        //Destroy(activeSkeleton[kvp.Key]);
                        //PrintDebugMessage("I: Remove skeleton: " + kvp.Key);
                        activeSkeleton.Remove(kvp.Key);
                    }
                }
            }


            PrintDebugMessage("I: Update complete correctly!");
        
        
        }


        void SetParentValue(GameObject objectInput)
        {
            Animator animator = objectInput.GetComponent<Animator>();
            animator.transform.SetParent(anchorOrigin.transform);

            animator.GetBoneTransform(HumanBodyBones.Head).parent = anchorOrigin.transform;
            animator.GetBoneTransform(HumanBodyBones.Neck).parent = anchorOrigin.transform;
            animator.GetBoneTransform(HumanBodyBones.RightShoulder).parent = anchorOrigin.transform;
            animator.GetBoneTransform(HumanBodyBones.RightLowerArm).parent = anchorOrigin.transform;
            animator.GetBoneTransform(HumanBodyBones.RightHand).parent = anchorOrigin.transform;
            animator.GetBoneTransform(HumanBodyBones.LeftShoulder).parent = anchorOrigin.transform;
            animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).parent = anchorOrigin.transform;
            animator.GetBoneTransform(HumanBodyBones.LeftHand).parent = anchorOrigin.transform;
            animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).parent = anchorOrigin.transform;
            animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).parent = anchorOrigin.transform;
            animator.GetBoneTransform(HumanBodyBones.RightFoot).parent = anchorOrigin.transform;
            animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).parent = anchorOrigin.transform;
            animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).parent = anchorOrigin.transform;
            animator.GetBoneTransform(HumanBodyBones.LeftFoot).parent = anchorOrigin.transform;
            animator.GetBoneTransform(HumanBodyBones.Spine).parent = anchorOrigin.transform;
            animator.GetBoneTransform(HumanBodyBones.Hips).parent = anchorOrigin.transform;

        }

        void SetJointsValue(GameObject objectInput, Vector3[] poseInput)
        {
            Animator animator = objectInput.GetComponent<Animator>();
            animator.transform.SetParent(anchorOrigin.transform);

            animator.GetBoneTransform(HumanBodyBones.Head).localPosition = new Vector3(poseInput[0].x, poseInput[0].y + 0.2f, poseInput[0].z);
            animator.GetBoneTransform(HumanBodyBones.Neck).localPosition = poseInput[1];
            animator.GetBoneTransform(HumanBodyBones.RightShoulder).localPosition = poseInput[2];
            animator.GetBoneTransform(HumanBodyBones.RightLowerArm).localPosition = poseInput[3];
            animator.GetBoneTransform(HumanBodyBones.RightHand).localPosition = poseInput[4];
            animator.GetBoneTransform(HumanBodyBones.LeftShoulder).localPosition = poseInput[5];
            animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).localPosition = poseInput[6];
            animator.GetBoneTransform(HumanBodyBones.LeftHand).localPosition = poseInput[7];
            animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).localPosition = poseInput[8];
            animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).localPosition = poseInput[9];
            animator.GetBoneTransform(HumanBodyBones.RightFoot).localPosition = poseInput[10];
            animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).localPosition = poseInput[11];
            animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).localPosition = poseInput[12];
            animator.GetBoneTransform(HumanBodyBones.LeftFoot).localPosition = poseInput[13];

            animator.GetBoneTransform(HumanBodyBones.Spine).localPosition = new Vector3(poseInput[14].x, poseInput[14].y + 0.15f, poseInput[14].z);

            Vector3 poseHips = new Vector3((poseInput[8].x + poseInput[11].x) / 2, (poseInput[8].y + poseInput[11].y) / 2 + 0.05f, (poseInput[8].z + poseInput[11].z) / 2);
            animator.GetBoneTransform(HumanBodyBones.Hips).localPosition = poseHips;
        }





        /// <summary>
        /// Losts the position, reset all.
        /// </summary>
        private void LostPosition(string message)
        {
            PrintDebugMessage("E: Position Lost! " + message);
            Destroy(anchorOrigin);
            Destroy(anchorOriginObject);

            if (activeSkeleton.Count > 0)
            {
                foreach (KeyValuePair<int, GameObject> kvp in activeSkeleton)
                {
                    activeSkeleton[kvp.Key].SetActive(false);
                    //Destroy(activeSkeleton[kvp.Key]);
                    PrintDebugMessage("I: Remove skeleton: " + kvp.Key);
                    activeSkeleton.Remove(kvp.Key);
                }
            }

            //foreach (KeyValuePair<int, GameObject> kvp in activeTracks)
            //{
            //    Destroy(activeTracks[kvp.Key]);
            //    PrintDebugMessage("I: Destroy centroid: " + kvp.Key);
            //    activeTracks.Remove(kvp.Key);
            //}
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

                    anchorOriginObject = Instantiate(anchorObject);
                    anchorOriginObject.transform.parent = anchorOrigin.transform;
                    anchorOriginObject.transform.localPosition = Vector3.zero;
                    anchorOriginObject.transform.localRotation = Quaternion.identity;

                    PrintDebugMessage("I: Anchor and Image created!");
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

        /// <summary>
        /// Creates the particle system.
        /// </summary>
        /// <returns>The particle system.</returns>
        /// <param name="newCentroid">Parent Centroid.</param>
        /// <param name="color">Centroid Color.</param>
        private ParticleSystem CreateParticleSystem(GameObject newCentroid, Color color)
        {
            ParticleSystem newParticular = Instantiate(partSystem);
            var mainPartSyst = newParticular.main;
            mainPartSyst.duration = 5.00f;
            mainPartSyst.startDelay = 0f;
            mainPartSyst.startLifetime = 2.0f;
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

            return newParticular;
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

                    particles.Add(track.Key, CreateParticleSystem(newCentroid, color));
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
                    Destroy(particles[kvp.Key]);
                    Destroy(activeTracks[kvp.Key]);
                    PrintDebugMessage("I: Remove centroid and particles: " + kvp.Key);
                    activeTracks.Remove(kvp.Key);
                    particles.Remove(kvp.Key);
                }
            }
        }

    }
}