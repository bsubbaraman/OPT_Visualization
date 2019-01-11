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


using System.Collections.Generic;
using GoogleARCore;
using UnityEngine;
using System;
using System.Threading;


namespace RosSharp.RosBridgeClient
{
    /// <summary>
    /// Controller for AugmentedImage example.
    /// </summary>
    public class Visualization : MonoBehaviour
    {
        public GameObject ServerConnection;
        public GameObject anchorObject;
        public GameObject emptyGameObject;
        public GameObject centroidObject;
        public GameObject avatarPrefab;
        public GameObject objectPrefab;
        public GameObject LabelTemplate;
        public GameObject GUI;

        public ParticleSystem partSystem;

        public Dictionary<int, GameObject> activeTracks = new Dictionary<int, GameObject>(); //person tracks
        public Dictionary<int, ParticleSystem> particles = new Dictionary<int, ParticleSystem>();
        public Dictionary<int, GameObject> activeSkeleton = new Dictionary<int, GameObject>();
        public Dictionary<int, GameObject> activeObjects = new Dictionary<int, GameObject>();
        public Dictionary<int, GameObject> labels = new Dictionary<int, GameObject>();
        private Dictionary<int, Color> colors = new Dictionary<int, Color>();


        public bool centroidView;
        public bool skeletonView;
        public bool objectView;
        private bool changeMode;

        /// <summary>
        /// The anchor origin.
        /// </summary>
        //private Anchor anchorOrigin = null;

        //private Vector3 positionCentreImage;
        //private Quaternion rotationCentreImage;

        //public String ipAddress;

        int counterErrorDistance = 0;

        const float DISTANCE_ANGLE = 4.0f;
        const float DISTANCE_METER = 0.05f;

        public RosSharp.RosBridgeClient.RosConnector rosConnector;
        public RosSharp.RosBridgeClient.CentroidSubscriber centroidSub;
        public RosSharp.RosBridgeClient.SkeletonSubscriber skeletonSub;
        public RosSharp.RosBridgeClient.ObjectsSubscriber objectSub;
        public RosSharp.RosBridgeClient.PoseStampedPublisher posePub;


        private void Start()
        {
            PrintDebugMessage("D: -------- New execution --------");
            centroidView = false;
        }

        private void ConnectionToRos()
        {
            ////Destroy(rosPrivate);
            //rosConnector.TearDown();
            //PrintDebugMessage("ROS - E :  Server not running on: " + rosConnector.RosBridgeServerUrl);
            //rosConnector.SetAddress("ws://" + ipAddress + ":9090");
            //rosConnector.Awake();
            ////rosPrivate = Instantiate(ros);
        }

        //public Texture skeletonTexture;
        //public Texture centroidTexture;


        //public void OnGUI()
        //{
        //    //if (!skeletonTexture || !centroidTexture)
        //    //{
        //    //    PrintDebugMessage("E: Texture not assigned in the ispector");
        //    //    return;
        //    //}

        //    //if (anchorOrigin == null)
        //    //return;


        //    if (GUI.Button(new Rect(0,0,100,100), "text")){
        //        Debug.Log("click!");
        //    }

        //    if (centroidView)
        //    {
        //        //if (GUI.Button(new Rect(8, Screen.height-200, 110, 200), skeletonTexture))
        //        //{
        //        //    centroidView = false;
        //        //    changeMode = true;

        //        //    PrintDebugMessage("I: SWAP -> Skeleton mode");
        //        //    return;

        //        //}
        //    }
        //    else
        //    {
        //        //if (GUI.Button(new Rect(8, Screen.height-110, 200, 110), centroidTexture))
        //        //{
        //        //    centroidView = true;
        //        //    changeMode = true;

        //        //    PrintDebugMessage("I: SWAP -> Centroid mode");
        //        //    return;

        //        //}
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

            // To Open Control Panel

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                GUI.SetActive(!GUI.activeSelf);
            }

            //if (Session.Status != SessionStatus.Tracking && counterErrorDistance < 5)
            //{
            //    counterErrorDistance++;
            //    PrintDebugMessage("W: Counter lost tracking #" + counterErrorDistance);
            //    return;
            //}

            if (!rosConnector.ConnectionStatus())
            {
                ServerConnection.SetActive(true);
                //ConnectionToRos();
                return;
            }

            //Search anchor
            //if (anchorOrigin == null)
            //{
            //    ServerConnection.SetActive(false);
            //    FitToScanOverlay.SetActive(true);

            //    SearchAnchorOrigin();

            //    return;
            //}

            //control if the tracking is lost for at least 5 times
            if (counterErrorDistance > 4)
            {
                //LostPosition("Tracking problem", true);
                counterErrorDistance = 0;
                return;
            }
            counterErrorDistance = 0;

            if (changeMode)
            {
                //LostPosition("Change mode", false);
                changeMode = false;
                return;
            }

            //Send the pose to ROS
            //PoseSender();
            //if (centroidView)
            if (centroidView)
            {
                CreateCentroidFromRosData();
            }
            else
            {
                RemoveAllCentroids();
            }
            if (skeletonView)
            {
                CreateSkeletonFromRosData();
            }
            else
            {
                RemoveAllSkeletons();
            }
            if (objectView)
            {
                CreateObjectMarkersFromRosData();
            }
            else
            {
                RemoveAllObjects();
            }
            PrintDebugMessage("I: Update complete correctly!");

        }


        /// <summary>
        /// Losts the position, reset all.
        /// </summary>
        //private void LostPosition(string message, bool action)
        //{
        //    PrintDebugMessage("E: Position Lost! " + message);

        //    if(action)
        //    {
        //        Destroy(anchorOrigin);
        //        Destroy(anchorOriginObject);
        //        counterErrorDistance = 0;
        //    }

        //    RemoveAllCentroids();
        //    RemoveAllSkeletons();
        //}

        /// <summary>
        /// Removes all centroids.
        /// </summary>
        public void RemoveAllCentroids()
        {
            if (activeTracks.Count > 0)
            {
                foreach (KeyValuePair<int, GameObject> kvp in activeTracks)
                {
                    if (particles[kvp.Key])
                    {
                        Destroy(particles[kvp.Key]);
                        particles.Remove(kvp.Key);
                    }
                    if (activeTracks[kvp.Key])
                    {
                        Destroy(activeTracks[kvp.Key]);
                        activeTracks.Remove(kvp.Key);
                    }

                    PrintDebugMessage("I: Destroy centroid and particles: " + kvp.Key);
                }
            }

        }

        /// <summary>
        /// Removes all skeletons.
        /// </summary>
        public void RemoveAllSkeletons()
        {
            if (activeSkeleton.Count > 0)
            {
                foreach (KeyValuePair<int, GameObject> kvp in activeSkeleton)
                {
                    //activeSkeleton[kvp.Key].SetActive(false);
                    if (activeSkeleton[kvp.Key])
                    {
                        Destroy(activeSkeleton[kvp.Key]);
                        activeSkeleton.Remove(kvp.Key);
                        PrintDebugMessage("I: Remove skeleton: " + kvp.Key);
                    }

                }
            }

        }

        public void RemoveAllObjects()
        {
            if (activeObjects.Count > 0)
            {
                foreach (KeyValuePair<int, GameObject> kvp in activeObjects)
                {
                    //if (particles[kvp.Key])
                    //{
                    //    Destroy(particles[kvp.Key]);
                    //    particles.Remove(kvp.Key);
                    //}
                    if (activeObjects[kvp.Key])
                    {
                        Destroy(activeObjects[kvp.Key]);
                        activeObjects.Remove(kvp.Key);
                    }

                    PrintDebugMessage("I: Destroy object: " + kvp.Key);
                }
            }

        }


        /// <summary>
        /// Prints the debug message.
        /// </summary>
        /// <param name="message">Message.</param>
        private void PrintDebugMessage(string message)
        {
            //Debug.Log("123 - " + message);
        }

        /// <summary>
        /// Searchs the anchor origin.
        /// </summary>
        //private void SearchAnchorOrigin()
        //{
        //    // Get updated augmented images for this frame.
        //    Session.GetTrackables<AugmentedImage>(m_TempAugmentedImages, TrackableQueryFilter.Updated);

        //    // Create visualizers and anchors for updated augmented images that are tracking and do not previously
        //    // have a visualizer. Remove visualizers for stopped images.
        //    foreach (var image in m_TempAugmentedImages)
        //    {
        //        if (image.TrackingState == TrackingState.Tracking)
        //        {
        //            // Create an anchor to ensure that ARCore keeps tracking this augmented image.
        //            anchorOrigin = image.CreateAnchor(image.CenterPose);

        //            positionCentreImage = image.CenterPose.position;
        //            rotationCentreImage = image.CenterPose.rotation;

        //            PrintDebugMessage("I: Position: P:" + image.CenterPose.position.ToString() + " , quat: " + image.CenterPose.rotation.ToString());

        //            FitToScanOverlay.SetActive(false);

        //            anchorOriginObject = Instantiate(anchorObject);
        //            anchorOriginObject.transform.parent = anchorOrigin.transform;
        //            anchorOriginObject.transform.localPosition = Vector3.zero;
        //            anchorOriginObject.transform.localRotation = Quaternion.identity;

        //            PrintDebugMessage("I: Anchor and Image created!");
        //            //PrintDebugMessage("I: " + image.ExtentX + " H: " + image.ExtentZ);
        //        }
        //    }
        //}

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
        //private void PoseSender()
        //{
        //    GameObject cameraObject = Instantiate(emptyGameObject, FirstPersonCamera.transform.position, FirstPersonCamera.transform.rotation);
        //    cameraObject.transform.SetParent(anchorOrigin.transform);

        //    posePub.SendMessage(SwapCoordinates(cameraObject.transform.localPosition, cameraObject.transform.localRotation), SystemInfo.deviceUniqueIdentifier);

        //    Destroy(cameraObject);
        //}

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
            newParticular.Stop(); //can't set duration while playing
            mainPartSyst.duration = 5.00f;
            newParticular.Play();
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
        /// Creates label for all markers
        /// </summary>

        private GameObject CreateLabel(GameObject theMarker, string info)
        {
            GameObject label = Instantiate(LabelTemplate);
            TextMesh tm = label.GetComponent<TextMesh>();
            label.transform.SetParent(theMarker.transform);
            tm.text = info;
            tm.transform.localPosition = new Vector3(0f, 1f, 0f); // to position just above marker
            tm.transform.localScale = new Vector3(1f, 1f, 1f);


            return label;
        }


        /// <summary>
        /// Creates the centroids from ros data.
        /// </summary>
        private void CreateCentroidFromRosData()
        {
            //Data from centroidSub
            Dictionary<int, Vector3> dataFromCentroidSub = centroidSub.processedTrackData;

            //Data from skeletonSub
            //Dictionary<int, Vector3> dataFromSkeletonSubCentroid = skeletonSub.centroidPose;
            PrintDebugMessage("I: Received data from CentroidSub length: " + dataFromCentroidSub.Count);

            foreach (KeyValuePair<int, Vector3> track in dataFromCentroidSub)
            {
                //int id = track.Key;
                //Vector3 poseInput = track.Value;

                //add any people who have joined the scene
                if (!activeTracks.ContainsKey(track.Key))
                {
                    Debug.Log(activeTracks.Count);
                    Color color = new Color(
                          UnityEngine.Random.Range(0f, 1f),
                          UnityEngine.Random.Range(0f, 1f),
                          UnityEngine.Random.Range(0f, 1f),
                                            1);

                    GameObject newCentroid = Instantiate(centroidObject);
                    //newCentroid.transform.SetParent(anchorOrigin.transform);
                    newCentroid.transform.localPosition = track.Value;
                    newCentroid.name = "centroid_" + track.Key;
                    newCentroid.GetComponent<Renderer>().material.color = color;

                    particles.Add(track.Key, CreateParticleSystem(newCentroid, color));
                    activeTracks.Add(track.Key, newCentroid);
                    labels.Add(track.Key, CreateLabel(newCentroid, track.Key.ToString()));


                    //add label


                    //PrintDebugMessage("I: Crete centroid -> Parent: " + activeTracks[id].transform.parent.name + " | Position: " + activeTracks[id].transform.localPosition.ToString() + " | Id: " + id);
                }
                else
                {
                    activeTracks[track.Key].transform.localPosition = track.Value;
                    //PrintDebugMessage("I: Update centroid  -> Parent: " + activeTracks[id].transform.parent.name + " | Position: " + activeTracks[id].transform.localPosition.ToString() + " | Id: " + id);
                }

            }

            //remove any people who are no longer present
            Dictionary<int, GameObject> checkActiveTracks = activeTracks;
            foreach (KeyValuePair<int, GameObject> kvp in checkActiveTracks)
            {
                if (!dataFromCentroidSub.ContainsKey(kvp.Key))
                {
                    if (particles[kvp.Key])
                    {
                        Destroy(particles[kvp.Key]);
                        particles.Remove(kvp.Key);
                    }
                    if (activeTracks[kvp.Key])
                    {
                        Destroy(activeTracks[kvp.Key]);
                        activeTracks.Remove(kvp.Key);
                    }
                }
                Debug.Log("****************** " + checkActiveTracks.Count);
            }
        }

        /// <summary>
        /// Creates the skeleton from ros data.
        /// </summary>
        void CreateSkeletonFromRosData()
        {

            Dictionary<int, Vector3[]> dataFromSkeletonSubSkeleton = skeletonSub.jointsData;
            //PrintDebugMessage("I: Received data from SkeletonSub length: " + dataFromSkeletonSubSkeleton.Count);

            foreach (KeyValuePair<int, Vector3[]> track in dataFromSkeletonSubSkeleton)
            {
                if (!activeSkeleton.ContainsKey(track.Key))
                {
                    GameObject newSkeleton = Instantiate(avatarPrefab);
                    newSkeleton.name = "Skeleton_" + track.Key;
                    activeSkeleton.Add(track.Key, newSkeleton);
                    PrintDebugMessage("I: Crete skeleton Id # " + track.Key);

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
                        if (activeSkeleton[kvp.Key])
                        {
                            Destroy(activeSkeleton[kvp.Key]);
                            activeSkeleton.Remove(kvp.Key);
                            PrintDebugMessage("I: Remove skeleton: " + kvp.Key);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the joints value.
        /// </summary>
        /// <param name="theAvatar">The avatar.</param>
        /// <param name="poseInput">Pose input.</param>
        void SetJointsValue(GameObject theAvatar, Vector3[] poseInput)
        {
            PrintDebugMessage("I: Processing: " + theAvatar.name);

            Animator animator = theAvatar.GetComponent<Animator>();

            //Matrix4x4 m = Matrix4x4.TRS(anchorOrigin.transform.position, anchorOrigin.transform.rotation, new Vector3(1, 1, 1));

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
            Debug.Log("CHEST VEC __________" + chest_vec);
            Debug.Log("CHEST __________" + chest.position);
            if (Vector3.Distance(chest_vec, chest.position) > DISTANCE_METER)
            {
                chest.position = chest_vec; //move avatar to correct location
            }

            Vector3 shoulder_shoulder_vec = l_shoulder_vec - r_shoulder_vec;
            quaternionValue = Quaternion.LookRotation(shoulder_shoulder_vec);
            quaternionValue *= Quaternion.Euler(0.0f, 0.0f, -90.0f);

            PrintDebugMessage("I: " + Vector3.Distance(quaternionValue.eulerAngles, chest.rotation.eulerAngles) % 360);

            if (Vector3.Distance(quaternionValue.eulerAngles, chest.rotation.eulerAngles) % 360 > DISTANCE_ANGLE)
            {
                chest.rotation = quaternionValue;
            }

            Vector3 r_shoulder_elbow = r_elbow_vec - r_shoulder_vec;
            quaternionValue = XLookRotation(r_shoulder_elbow);
            quaternionValue *= Quaternion.Euler(180.0f, 0.0f, 0.0f);
            PrintDebugMessage("I: " + Vector3.Distance(quaternionValue.eulerAngles, right_shoulder.rotation.eulerAngles) % 360);
            if (Vector3.Distance(quaternionValue.eulerAngles, right_shoulder.rotation.eulerAngles) % 360 > DISTANCE_ANGLE)
            {
                right_shoulder.rotation = quaternionValue;
            }

            // place elbow correctly
            Vector3 r_elbow_wrist = r_wrist_vec - r_elbow_vec;
            quaternionValue = XLookRotation(r_elbow_wrist);
            PrintDebugMessage("I: " + Vector3.Distance(quaternionValue.eulerAngles, right_elbow.rotation.eulerAngles) % 360);
            if (Vector3.Distance(quaternionValue.eulerAngles, right_elbow.rotation.eulerAngles) % 360 > DISTANCE_ANGLE)
            {
                right_elbow.rotation = quaternionValue;
            }


            //Left upper body:
            Vector3 l_shoulder_elbow = l_shoulder_vec - l_elbow_vec;
            quaternionValue = XLookRotation(l_shoulder_elbow);
            PrintDebugMessage("I: " + Vector3.Distance(quaternionValue.eulerAngles, left_shoulder.rotation.eulerAngles) % 360);
            if (Vector3.Distance(quaternionValue.eulerAngles, left_shoulder.rotation.eulerAngles) % 360 > DISTANCE_ANGLE)
            {
                left_shoulder.rotation = quaternionValue; //function defined below.  artifact of how avatar is rotated
            }

            // place elbow correctly
            Vector3 l_elbow_wrist = l_elbow_vec - l_wrist_vec;
            quaternionValue = YLookRotation(l_elbow_wrist);
            PrintDebugMessage("I: " + Vector3.Distance(quaternionValue.eulerAngles, left_elbow.rotation.eulerAngles) % 360);
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
            PrintDebugMessage("I: " + Vector3.Distance(quaternionValue.eulerAngles, hip.rotation.eulerAngles) % 360);
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
            PrintDebugMessage("I: " + Vector3.Distance(quaternionValue.eulerAngles, left_hip.rotation.eulerAngles) % 360);
            if (Vector3.Distance(quaternionValue.eulerAngles, left_hip.rotation.eulerAngles) % 360 > DISTANCE_ANGLE)
            {
                left_hip.rotation = quaternionValue;
                Vector3 localZ = left_hip.TransformDirection(Vector3.forward);
                if (Vector3.Dot(hip_hip_vec, localZ) > 0)
                {
                    PrintDebugMessage("I: Enter in refinement");
                    left_hip.RotateAround(left_hip.position, left_hip.right, 180f);
                }
            }


            Vector3 l_knee_ankle = l_knee_vec - l_ankle_vec;
            quaternionValue = XLookRotation(l_knee_ankle);
            PrintDebugMessage("I: " + Vector3.Distance(quaternionValue.eulerAngles, left_knee.rotation.eulerAngles) % 360);
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
            PrintDebugMessage("I: " + Vector3.Distance(quaternionValue.eulerAngles, right_hip.rotation.eulerAngles) % 360);
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
            PrintDebugMessage("I: " + Vector3.Distance(quaternionValue.eulerAngles, right_knee.rotation.eulerAngles) % 360);
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

        /// <summary>
        /// XLs the ook rotation.
        /// </summary>
        /// <returns>The X look rotation.</returns>
        /// <param name="right">Right.</param>
        Quaternion XLookRotation(Vector3 right)
        {
            Quaternion rightToForward = Quaternion.Euler(0f, -90f, 0f);
            Quaternion forwardToTarget = Quaternion.LookRotation(right, Vector3.up);

            return forwardToTarget * rightToForward;
        }

        /// <summary>
        /// YLs the look rotation.
        /// </summary>
        /// <returns>The look rotation.</returns>
        /// <param name="up">Up.</param>
        Quaternion YLookRotation(Vector3 up)
        {
            Quaternion upToForward = Quaternion.Euler(90f, 0f, 0f);
            Quaternion forwardToTarget = Quaternion.LookRotation(up, Vector3.up);

            return forwardToTarget * upToForward;
        }

        // *** Adding Object Markers

        private void CreateObjectMarkersFromRosData()
        {
            //Data from centroidSub
            //Dictionary<int, Vector3> dataFromObjectSub = objectSub.objectTrackData;
            Dictionary<int, OPTObject> dataFromObjectSub = objectSub.objectTrackData;
            //Debug.Log(dataFromObjectSub.Count);
            //Data from skeletonSub
            //Dictionary<int, Vector3> dataFromSkeletonSubCentroid = skeletonSub.centroidPose;
            PrintDebugMessage("I: Received data from objectSub length: " + dataFromObjectSub.Count);

            foreach (KeyValuePair<int, OPTObject> track in dataFromObjectSub)
            {
                //int id = track.Key;
                //Vector3 poseInput = track.Value;

                //add any object which have joined the scene
                if (!activeObjects.ContainsKey(track.Key))
                {
                    Color color = new Color(
                          UnityEngine.Random.Range(0f, 1f),
                          UnityEngine.Random.Range(0f, 1f),
                          UnityEngine.Random.Range(0f, 1f),
                                            1);

                    GameObject newObject = Instantiate(objectPrefab);
                    //newCentroid.transform.SetParent(anchorOrigin.transform);
                    newObject.transform.localPosition = track.Value.pos;
                    newObject.name = "object_" + track.Key;
                    newObject.GetComponent<Renderer>().material.color = color;

                    particles.Add(track.Key, CreateParticleSystem(newObject, color));
                    activeObjects.Add(track.Key, newObject);
                    labels.Add(track.Key, CreateLabel(newObject, track.Value.objectID));

                    //PrintDebugMessage("I: Crete centroid -> Parent: " + activeTracks[id].transform.parent.name + " | Position: " + activeTracks[id].transform.localPosition.ToString() + " | Id: " + id);
                }
                else
                {
                    activeObjects[track.Key].transform.localPosition = track.Value.pos;
                    //PrintDebugMessage("I: Update centroid  -> Parent: " + activeTracks[id].transform.parent.name + " | Position: " + activeTracks[id].transform.localPosition.ToString() + " | Id: " + id);
                }

            }

            //remove any objects which are no longer present
            foreach (KeyValuePair<int, GameObject> kvp in activeObjects)
            {
                if (!dataFromObjectSub.ContainsKey(kvp.Key))
                {
                    if (particles[kvp.Key])
                    {
                        Destroy(particles[kvp.Key]);
                        particles.Remove(kvp.Key);
                    }
                    if (activeObjects[kvp.Key])
                    {
                        Destroy(activeObjects[kvp.Key]);
                        activeObjects.Remove(kvp.Key);
                    }
                }
            }
        }

    }
}