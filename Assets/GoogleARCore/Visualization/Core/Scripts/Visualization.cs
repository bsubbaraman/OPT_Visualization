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

        /// <summary>
        /// The overlay containing the fit to scan user guide.
        /// </summary>
        public GameObject FitToScanOverlay;
        public GameObject ServerConnection;
        public GameObject anchorObject;
        public GameObject emptyGameObject;
        public GameObject centroidObject;
        private GameObject anchorOriginObject;
        public GameObject avatarPrefab;

        public ParticleSystem partSystem;

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

        const float DISTANCE_ANGLE = 4.0f;
        const float DISTANCE_METER = 0.05f;

        public RosSharp.RosBridgeClient.RosConnector rosConnector;
        public RosSharp.RosBridgeClient.CentroidSubscriber centroidSub;
        public RosSharp.RosBridgeClient.SkeletonSubscriber skeletonSub;
        public RosSharp.RosBridgeClient.PoseStampedPublisher posePub;
        public RosSharp.RosBridgeClient.PoseStampedPublisher posePubOrigin;


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
                if (!activeSkeleton.ContainsKey(track.Key))
                {
                    GameObject newSkeleton = Instantiate(avatarPrefab);
                    //newSkeleton.transform.SetParent(anchorOrigin.transform);
                    newSkeleton.name = "Skeleton_" + track.Key;
                    //SetParentValue(newSkeleton);
                    activeSkeleton.Add(track.Key, newSkeleton);
                    Debug.Log("I: Crete skeleton Id # " + track.Key);

                }

                SetJointsValue(activeSkeleton[track.Key], dataFromSkeletonSubSkeleton[track.Key]);

            }

            //remove any people who are no longer present
            if (activeSkeleton.Count > 0)
            {
                foreach (KeyValuePair<int, GameObject> kvp in activeSkeleton)
                {
                    if (!dataFromSkeletonSubSkeleton.ContainsKey(kvp.Key))
                    {
                        //activeSkeleton[kvp.Key].SetActive(false);
                        Destroy(activeSkeleton[kvp.Key]);
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

        //void SetJointsValue(GameObject objectInput, Vector3[] poseInput)
        //{
        //    Animator animator = objectInput.GetComponent<Animator>();
        //    animator.transform.SetParent(anchorOrigin.transform);

        //    animator.GetBoneTransform(HumanBodyBones.Head).localPosition = new Vector3(poseInput[0].x, poseInput[0].y + 0.2f, poseInput[0].z);
        //    animator.GetBoneTransform(HumanBodyBones.Neck).localPosition = poseInput[1];
        //    animator.GetBoneTransform(HumanBodyBones.RightShoulder).localPosition = poseInput[2];
        //    animator.GetBoneTransform(HumanBodyBones.RightLowerArm).localPosition = poseInput[3];
        //    animator.GetBoneTransform(HumanBodyBones.RightHand).localPosition = poseInput[4];
        //    animator.GetBoneTransform(HumanBodyBones.LeftShoulder).localPosition = poseInput[5];
        //    animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).localPosition = poseInput[6];
        //    animator.GetBoneTransform(HumanBodyBones.LeftHand).localPosition = poseInput[7];
        //    animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).localPosition = poseInput[8];
        //    animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).localPosition = poseInput[9];
        //    animator.GetBoneTransform(HumanBodyBones.RightFoot).localPosition = poseInput[10];
        //    animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).localPosition = poseInput[11];
        //    animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).localPosition = poseInput[12];
        //    animator.GetBoneTransform(HumanBodyBones.LeftFoot).localPosition = poseInput[13];

        //    animator.GetBoneTransform(HumanBodyBones.Spine).localPosition = new Vector3(poseInput[14].x, poseInput[14].y + 0.15f, poseInput[14].z);

        //    Vector3 poseHips = new Vector3((poseInput[8].x + poseInput[11].x) / 2, (poseInput[8].y + poseInput[11].y) / 2 + 0.05f, (poseInput[8].z + poseInput[11].z) / 2);
        //    animator.GetBoneTransform(HumanBodyBones.Hips).localPosition = poseHips;
        //}

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

        void SetJointsValue(GameObject theAvatar, Vector3[] poseInput)
        {
            Animator animator = theAvatar.GetComponent<Animator>();

            Matrix4x4 m = Matrix4x4.TRS(anchorOrigin.transform.position, anchorOrigin.transform.rotation, new Vector3(1, 1, 1));

            Vector3 chest_vec = m.MultiplyPoint3x4(poseInput[14]);
            Vector3 l_knee_vec = m.MultiplyPoint3x4(poseInput[12]);
            Vector3 l_hip_vec = m.MultiplyPoint3x4(poseInput[11]);
            Vector3 r_hip_vec = m.MultiplyPoint3x4(poseInput[8]);
            Vector3 l_ankle_vec = m.MultiplyPoint3x4(poseInput[13]);
            Vector3 r_knee_vec = m.MultiplyPoint3x4(poseInput[9]);
            Vector3 r_ankle_vec = m.MultiplyPoint3x4(poseInput[10]);
            Vector3 l_shoulder_vec = m.MultiplyPoint3x4(poseInput[5]);
            Vector3 l_elbow_vec = m.MultiplyPoint3x4(poseInput[6]);
            Vector3 l_wrist_vec = m.MultiplyPoint3x4(poseInput[7]);
            Vector3 r_shoulder_vec = m.MultiplyPoint3x4(poseInput[2]);
            Vector3 r_elbow_vec = m.MultiplyPoint3x4(poseInput[3]);
            Vector3 r_wrist_vec = m.MultiplyPoint3x4(poseInput[4]);
            Vector3 neck_vec = m.MultiplyPoint3x4(poseInput[1]);
            Vector3 head_vec = m.MultiplyPoint3x4(poseInput[0]);

            //Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
            //Transform neck = animator.GetBoneTransform(HumanBodyBones.Neck);
            Transform right_shoulder = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            Transform right_elbow = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            //Transform right_hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            Transform left_shoulder = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            Transform left_elbow = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            //Transform left_hand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            Transform right_hip = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            Transform right_knee = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            Transform right_foot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
            Transform left_hip = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            Transform left_knee = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            Transform left_foot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            Transform hip = animator.GetBoneTransform(HumanBodyBones.Hips);
            Transform chest = animator.GetBoneTransform(HumanBodyBones.Spine);

            Quaternion quaternionValue;

            if (Vector3.Distance(chest_vec, chest.position) > DISTANCE_METER)
            {
                chest.position = chest_vec; //move avatar to correct location
            }

            Vector3 shoulder_shoulder_vec = l_shoulder_vec - r_shoulder_vec;
            quaternionValue = Quaternion.LookRotation(shoulder_shoulder_vec);
            quaternionValue *= Quaternion.Euler(0.0f, 0.0f, -90.0f);

            Debug.Log("-------------");
            Debug.Log(Vector3.Distance(quaternionValue.eulerAngles, chest.rotation.eulerAngles) % 360);

            if (Vector3.Distance(quaternionValue.eulerAngles, chest.rotation.eulerAngles) % 360 > DISTANCE_ANGLE)
            {
                chest.rotation = quaternionValue;
            }

            Vector3 r_shoulder_elbow = r_elbow_vec - r_shoulder_vec;
            quaternionValue = XLookRotation(r_shoulder_elbow);
            quaternionValue *= Quaternion.Euler(180.0f, 0.0f, 0.0f);
            Debug.Log(Vector3.Distance(quaternionValue.eulerAngles, right_shoulder.rotation.eulerAngles) % 360);
            if (Vector3.Distance(quaternionValue.eulerAngles, right_shoulder.rotation.eulerAngles) % 360 > DISTANCE_ANGLE)
            {
                right_shoulder.rotation = quaternionValue;
            }

            // place elbow correctly
            Vector3 r_elbow_wrist = r_wrist_vec - r_elbow_vec;
            quaternionValue = XLookRotation(r_elbow_wrist);
            Debug.Log(Vector3.Distance(quaternionValue.eulerAngles, right_elbow.rotation.eulerAngles) % 360);
            if (Vector3.Distance(quaternionValue.eulerAngles, right_elbow.rotation.eulerAngles) % 360 > DISTANCE_ANGLE)
            {
                right_elbow.rotation = quaternionValue;
            }


            //Left upper body:
            Vector3 l_shoulder_elbow = l_shoulder_vec - l_elbow_vec;
            quaternionValue = XLookRotation(l_shoulder_elbow);
            Debug.Log(Vector3.Distance(quaternionValue.eulerAngles, left_shoulder.rotation.eulerAngles) % 360);
            if (Vector3.Distance(quaternionValue.eulerAngles, left_shoulder.rotation.eulerAngles) % 360 > DISTANCE_ANGLE)
            {
                left_shoulder.rotation = quaternionValue; //function defined below.  artifact of how avatar is rotated
            }

            // place elbow correctly
            Vector3 l_elbow_wrist = l_elbow_vec - l_wrist_vec;
            quaternionValue = YLookRotation(l_elbow_wrist);
            Debug.Log(Vector3.Distance(quaternionValue.eulerAngles, left_elbow.rotation.eulerAngles) % 360);
            if (Vector3.Distance(quaternionValue.eulerAngles, left_elbow.rotation.eulerAngles) % 360 > DISTANCE_ANGLE)
            {
                left_elbow.rotation = quaternionValue;
            }

            //placing & orienting hips
            Vector3 hip_midpoint = (l_hip_vec + r_hip_vec) / 2;
            if (Vector3.Distance(hip_midpoint, hip.position) > DISTANCE_METER)
            {
                hip.position = hip_midpoint;
            }

            Vector3 hip_hip_vec = r_hip_vec - l_hip_vec;
            quaternionValue = Quaternion.LookRotation(hip_hip_vec);
            quaternionValue *= Quaternion.Euler(0f, 180f, -90f);
            Debug.Log(Vector3.Distance(quaternionValue.eulerAngles, hip.rotation.eulerAngles) % 360);
            if (Vector3.Distance(quaternionValue.eulerAngles, hip.rotation.eulerAngles) % 360 > DISTANCE_ANGLE)
            {
                hip.rotation = quaternionValue;

                left_foot.rotation = Quaternion.LookRotation(hip_hip_vec);
                left_foot.rotation *= Quaternion.Euler(0f, 0f, +45f);

                right_foot.rotation = Quaternion.LookRotation(hip_hip_vec);
                right_foot.rotation *= Quaternion.Euler(0f, 0f, 225f);
            }

            //orient left leg:
            Vector3 l_hip_knee = l_hip_vec - l_knee_vec;
            quaternionValue = XLookRotation(l_hip_knee);
            Debug.Log(Vector3.Distance(quaternionValue.eulerAngles, left_hip.rotation.eulerAngles) % 360);
            if (Vector3.Distance(quaternionValue.eulerAngles, left_hip.rotation.eulerAngles) % 360 > DISTANCE_ANGLE)
            {
                left_hip.rotation = quaternionValue;
                Vector3 localZ = left_hip.TransformDirection(Vector3.forward);
                if (Vector3.Dot(hip_hip_vec, localZ) > 0)
                {
                    Debug.Log("Enter");
                    left_hip.RotateAround(left_hip.position, left_hip.right, 180f);
                }
            }


            Vector3 l_knee_ankle = l_knee_vec - l_ankle_vec;
            quaternionValue = XLookRotation(l_knee_ankle);
            Debug.Log(Vector3.Distance(quaternionValue.eulerAngles, left_knee.rotation.eulerAngles) % 360);
            if (Vector3.Distance(quaternionValue.eulerAngles, left_knee.rotation.eulerAngles) % 360 > DISTANCE_ANGLE)
            {
                left_knee.rotation = quaternionValue;
                Vector3 localZ = left_knee.TransformDirection(Vector3.forward);
                if (Vector3.Dot(hip_hip_vec, localZ) > 0)
                {
                    left_knee.RotateAround(left_knee.position, left_knee.right, 180f);
                }
            }

            //orient right leg:
            Vector3 r_hip_knee = r_knee_vec - r_hip_vec;
            quaternionValue = XLookRotation(r_hip_knee);
            Debug.Log(Vector3.Distance(quaternionValue.eulerAngles, right_hip.rotation.eulerAngles) % 360);
            if (Vector3.Distance(quaternionValue.eulerAngles, right_hip.rotation.eulerAngles) % 360 > DISTANCE_ANGLE)
            {
                right_hip.rotation = quaternionValue;
                Vector3 localZ = right_hip.TransformDirection(Vector3.forward);
                if (Vector3.Dot(hip_hip_vec, localZ) < 0)
                {
                    right_hip.RotateAround(right_hip.position, right_hip.right, 180f);
                }
            }
            Vector3 r_knee_ankle = r_ankle_vec - r_knee_vec;
            quaternionValue = XLookRotation(r_knee_ankle);
            Debug.Log(Vector3.Distance(quaternionValue.eulerAngles, right_knee.rotation.eulerAngles) % 360);
            if (Vector3.Distance(quaternionValue.eulerAngles, right_knee.rotation.eulerAngles) % 360 > DISTANCE_ANGLE)
            {
                right_knee.rotation = quaternionValue;
                Vector3 localZ = right_knee.TransformDirection(Vector3.forward);
                if (Vector3.Dot(hip_hip_vec, localZ) < 0)
                {
                    right_knee.RotateAround(right_knee.position, right_knee.right, 180f);
                }
            }

        }

        void SetJointsValue2(GameObject theAvatar, Vector3[] poseInput)
        {
            Animator animator = theAvatar.GetComponent<Animator>();

            Vector3 chest_vec = poseInput[14];
            Vector3 l_knee_vec = poseInput[12];
            Vector3 l_hip_vec = poseInput[11];
            Vector3 r_hip_vec = poseInput[8];
            Vector3 l_ankle_vec = poseInput[13];
            Vector3 r_knee_vec = poseInput[9];
            Vector3 r_ankle_vec = poseInput[10];
            Vector3 l_shoulder_vec = poseInput[5];
            Vector3 l_elbow_vec = poseInput[6];
            Vector3 l_wrist_vec = poseInput[7];
            Vector3 r_shoulder_vec = poseInput[2];
            Vector3 r_elbow_vec = poseInput[3];
            Vector3 r_wrist_vec = poseInput[4];
            Vector3 neck_vec = poseInput[1];
            Vector3 head_vec = poseInput[0];

            //Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
            //Transform neck = animator.GetBoneTransform(HumanBodyBones.Neck);
            Transform right_shoulder = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            Transform right_elbow = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            //Transform right_hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            Transform left_shoulder = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            Transform left_elbow = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            //Transform left_hand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            Transform right_hip = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            Transform right_knee = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            Transform right_foot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
            Transform left_hip = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            Transform left_knee = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            Transform left_foot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            Transform hip = animator.GetBoneTransform(HumanBodyBones.Hips);
            Transform chest = animator.GetBoneTransform(HumanBodyBones.Spine);

            Quaternion quaternionValue;

            if (Vector3.Distance(chest_vec, chest.position) > DISTANCE_METER)
            {
                chest.localPosition = chest_vec; //move avatar to correct location
            }

            Vector3 shoulder_shoulder_vec = l_shoulder_vec - r_shoulder_vec;
            quaternionValue = Quaternion.LookRotation(shoulder_shoulder_vec);
            quaternionValue *= Quaternion.Euler(0.0f, 0.0f, -90.0f);

            Debug.Log("-------------");
            Debug.Log(Vector3.Distance(quaternionValue.eulerAngles, chest.localRotation.eulerAngles) % 360);

            if (Vector3.Distance(quaternionValue.eulerAngles, chest.localRotation.eulerAngles) % 360 > DISTANCE_ANGLE)
            {
                chest.localRotation = quaternionValue;
            }

            Vector3 r_shoulder_elbow = r_elbow_vec - r_shoulder_vec;
            quaternionValue = XLookRotation(r_shoulder_elbow);
            quaternionValue *= Quaternion.Euler(180.0f, 0.0f, 0.0f);
            Debug.Log(Vector3.Distance(quaternionValue.eulerAngles, right_shoulder.localRotation.eulerAngles) % 360);
            if (Vector3.Distance(quaternionValue.eulerAngles, right_shoulder.localRotation.eulerAngles) % 360 > DISTANCE_ANGLE)
            {
                right_shoulder.localRotation = quaternionValue;
            }

            // place elbow correctly
            Vector3 r_elbow_wrist = r_wrist_vec - r_elbow_vec;
            quaternionValue = XLookRotation(r_elbow_wrist);
            Debug.Log(Vector3.Distance(quaternionValue.eulerAngles, right_elbow.localRotation.eulerAngles) % 360);
            if (Vector3.Distance(quaternionValue.eulerAngles, right_elbow.localRotation.eulerAngles) % 360 > DISTANCE_ANGLE)
            {
                right_elbow.localRotation = quaternionValue;
            }


            //Left upper body:
            Vector3 l_shoulder_elbow = l_shoulder_vec - l_elbow_vec;
            quaternionValue = XLookRotation(l_shoulder_elbow);
            Debug.Log(Vector3.Distance(quaternionValue.eulerAngles, left_shoulder.localRotation.eulerAngles) % 360);
            if (Vector3.Distance(quaternionValue.eulerAngles, left_shoulder.localRotation.eulerAngles) % 360 > DISTANCE_ANGLE)
            {
                left_shoulder.localRotation = quaternionValue; //function defined below.  artifact of how avatar is rotated
            }

            // place elbow correctly
            Vector3 l_elbow_wrist = l_elbow_vec - l_wrist_vec;
            quaternionValue = YLookRotation(l_elbow_wrist);
            Debug.Log(Vector3.Distance(quaternionValue.eulerAngles, left_elbow.localRotation.eulerAngles) % 360);
            if (Vector3.Distance(quaternionValue.eulerAngles, left_elbow.localRotation.eulerAngles) % 360 > DISTANCE_ANGLE)
            {
                left_elbow.localRotation = quaternionValue;
            }

            //placing & orienting hips
            Vector3 hip_midpoint = (l_hip_vec + r_hip_vec) / 2;
            if (Vector3.Distance(hip_midpoint, hip.position) > DISTANCE_METER)
            {
                hip.position = hip_midpoint;
            }

            Vector3 hip_hip_vec = r_hip_vec - l_hip_vec;
            quaternionValue = Quaternion.LookRotation(hip_hip_vec);
            quaternionValue *= Quaternion.Euler(0f, 180f, -90f);
            Debug.Log(Vector3.Distance(quaternionValue.eulerAngles, hip.localRotation.eulerAngles) % 360);
            if (Vector3.Distance(quaternionValue.eulerAngles, hip.localRotation.eulerAngles) % 360 > DISTANCE_ANGLE)
            {
                hip.localRotation = quaternionValue;

                left_foot.localRotation = Quaternion.LookRotation(hip_hip_vec);
                left_foot.localRotation *= Quaternion.Euler(0f, 0f, +45f);

                right_foot.localRotation = Quaternion.LookRotation(hip_hip_vec);
                right_foot.localRotation *= Quaternion.Euler(0f, 0f, 225f);
            }

            //orient left leg:
            Vector3 l_hip_knee = l_hip_vec - l_knee_vec;
            quaternionValue = XLookRotation(l_hip_knee);
            Debug.Log(Vector3.Distance(quaternionValue.eulerAngles, left_hip.localRotation.eulerAngles) % 360);
            if (Vector3.Distance(quaternionValue.eulerAngles, left_hip.localRotation.eulerAngles) % 360 > DISTANCE_ANGLE)
            {
                left_hip.localRotation = quaternionValue;
                Vector3 localZ = left_hip.TransformDirection(Vector3.forward);
                if (Vector3.Dot(hip_hip_vec, localZ) > 0)
                {
                    Debug.Log("Enter");
                    left_hip.RotateAround(left_hip.position, left_hip.right, 180f);
                }
            }


            Vector3 l_knee_ankle = l_knee_vec - l_ankle_vec;
            quaternionValue = XLookRotation(l_knee_ankle);
            Debug.Log(Vector3.Distance(quaternionValue.eulerAngles, left_knee.localRotation.eulerAngles) % 360);
            if (Vector3.Distance(quaternionValue.eulerAngles, left_knee.localRotation.eulerAngles) % 360 > DISTANCE_ANGLE)
            {
                left_knee.localRotation = quaternionValue;
                Vector3 localZ = left_knee.TransformDirection(Vector3.forward);
                if (Vector3.Dot(hip_hip_vec, localZ) > 0)
                {
                    left_knee.RotateAround(left_knee.position, left_knee.right, 180f);
                }
            }

            //orient right leg:
            Vector3 r_hip_knee = r_knee_vec - r_hip_vec;
            quaternionValue = XLookRotation(r_hip_knee);
            Debug.Log(Vector3.Distance(quaternionValue.eulerAngles, right_hip.localRotation.eulerAngles) % 360);
            if (Vector3.Distance(quaternionValue.eulerAngles, right_hip.localRotation.eulerAngles) % 360 > DISTANCE_ANGLE)
            {
                right_hip.localRotation = quaternionValue;
                Vector3 localZ = right_hip.TransformDirection(Vector3.forward);
                if (Vector3.Dot(hip_hip_vec, localZ) < 0)
                {
                    right_hip.RotateAround(right_hip.position, right_hip.right, 180f);
                }
            }
            Vector3 r_knee_ankle = r_ankle_vec - r_knee_vec;
            quaternionValue = XLookRotation(r_knee_ankle);
            Debug.Log(Vector3.Distance(quaternionValue.eulerAngles, right_knee.localRotation.eulerAngles) % 360);
            if (Vector3.Distance(quaternionValue.eulerAngles, right_knee.localRotation.eulerAngles) % 360 > DISTANCE_ANGLE)
            {
                right_knee.localRotation = quaternionValue;
                Vector3 localZ = right_knee.TransformDirection(Vector3.forward);
                if (Vector3.Dot(hip_hip_vec, localZ) < 0)
                {
                    right_knee.RotateAround(right_knee.position, right_knee.right, 180f);
                }
            }

        }

        Quaternion XLookRotation(Vector3 right)
        {
            Quaternion rightToForward = Quaternion.Euler(0f, -90f, 0f);
            Quaternion forwardToTarget = Quaternion.LookRotation(right, Vector3.up);

            return forwardToTarget * rightToForward;
        }
        Quaternion YLookRotation(Vector3 up)
        {
            Quaternion upToForward = Quaternion.Euler(90f, 0f, 0f);
            Quaternion forwardToTarget = Quaternion.LookRotation(up, Vector3.up);

            return forwardToTarget * upToForward;
        }

    }
}