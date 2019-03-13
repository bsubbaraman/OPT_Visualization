using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenPTrack;
namespace RosSharp.RosBridgeClient
{

    public class CentroidBeam : MonoBehaviour
    {
        //public Dictionary<int, GameObject> centroidTracks = new Dictionary<int, GameObject>(); // these centroids should be the skeleton centroids to match id with skeleton
        public Visualization v;
        public SkeletonCentroidsSubscriber centroidSub;
        public Dictionary<int, GameObject> beams = new Dictionary<int, GameObject>();
        public SkeletonSubscriber skeletonSub;
        public GameObject BeamPrefab;
        public GameObject personalGravitationField;

        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (!enabled)
            {
                OptLogger.info("Centroid Beam is not enabled");
                return;
            }
            OptLogger.info("Centroid Beam is enabled!");
            Dictionary<int, Vector3> activeCentroids = centroidSub.processedTrackData;

            foreach (KeyValuePair<int, Vector3> track in activeCentroids)
            {
                OptLogger.info("updating beams");

                //add any people who have joined the scene
                if (!beams.ContainsKey(track.Key))
                {
                    Color color = new Color(
                            Random.Range(0f, 1f),
                            Random.Range(0f, 1f),
                            Random.Range(0f, 1f),
                                            1);
                    OptLogger.info("creating beam");
                    GameObject newBeam = Instantiate(BeamPrefab);
                    var ps = newBeam.GetComponent<ParticleSystem>().main;
                    ps.startColor = color;
                    OptLogger.info("Instantiated beam");
                    newBeam.transform.position = track.Value;
                    newBeam.transform.rotation = SetBeamOrientation(track.Key);
                    OptLogger.info("pose set");
                    newBeam.name = "beam_" + track.Key;
                    OptLogger.info("color set");

                    /*
                        Trying addition of peronal grav field
                    */
                    //GameObject newGravField = Instantiate(personalGravitationField, newBeam.transform);

                    beams.Add(track.Key, newBeam);
                    OptLogger.info("Number of beams in dictionary AFTER ADDING: " + beams.Count);
                    OptLogger.info("created beam");
                    //OptarLogger.info("I: Create centroid -> Parent: " + activeTracks[track.Key].transform.parent.name + " | Position: " + activeTracks[track.Key].transform.localPosition.ToString() + " | Id: " + track.Key);
                }
                else
                {
                    OptLogger.info("updating existing beam");
                    beams[track.Key].transform.position = track.Value;
                    beams[track.Key].transform.rotation = SetBeamOrientation(track.Key);
                    OptLogger.info("updated");
                    //OptarLogger.info("I: Update centroid  -> Parent: " + activeTracks[track.Key].transform.parent.name + " | Position: " + activeTracks[track.Key].transform.localPosition.ToString() + " | Id: " + track.Key);
                }


                OptLogger.info("beam position is " + beams[track.Key].transform.position);
            }

            //trying explicit list of external force fields that can influence beam.
            //foreach (KeyValuePair<int, GameObject> i in beams)
            //{
            //    foreach (KeyValuePair<int, GameObject> j in beams)
            //    {
            //        if (i.Key != j.Key){
            //            OptLogger.info("i not equal j");
            //            ParticleSystemForceField gravField = j.Value.transform.GetChild(0).gameObject.GetComponent<ParticleSystemForceField>(); // grav field is attached to 1st child gameobject of beam
            //            OptLogger.info("got gravField");
            //            ParticleSystem ps = i.Value.GetComponent<ParticleSystem>();
            //            OptLogger.info("got ps");
            //            ps.externalForces.AddInfluence(gravField);
            //            OptLogger.info("Added new grav field to influencer cout");
            //        }
            //        else{
            //            OptLogger.info("i=j :(");
            //        }
            //    }
            //    OptLogger.info("The influencer count is "+i.Value.GetComponent<ParticleSystem>().externalForces.influenceCount);
            //}


            // Destroy if no longer there:
            List<int> keysToRemove = new List<int>();
            foreach (KeyValuePair<int, GameObject> kvp in beams)
            {
                if (!activeCentroids.ContainsKey(kvp.Key))
                {
                        keysToRemove.Add(kvp.Key);
                }
            }
            removeSkeletons(keysToRemove);

            OptLogger.info("BEAMCOUNT: There are " + beams.Count + " beams in the scene");
        }

        private void removeSkeletons(List<int> keysToRemove)
        {
            foreach (int key in keysToRemove)
            {
                Destroy(beams[key]);
                beams.Remove(key);
            }
        }

        private Quaternion SetBeamOrientation(int id){
            Dictionary<int, Vector3[]> dataFromSkeletonSub = skeletonSub.jointsData;
            Vector3[] jointsPoses;
            try
            {
                jointsPoses = dataFromSkeletonSub[id];
            }
            catch (KeyNotFoundException e)
            {
                OptLogger.info("key " + id + " not found");
                return Quaternion.identity;
            }
            // convert to unity frame
           

            Vector3 l_shoulder_vec = jointsPoses[5];
            Vector3 r_shoulder_vec = jointsPoses[2];
            Vector3 shoulder_shoulder_vec = r_shoulder_vec - l_shoulder_vec;


            return XLookRotation(shoulder_shoulder_vec);
        }

        private static Quaternion XLookRotation(Vector3 right)
        {
            Quaternion rightToForward = Quaternion.Euler(0f, -90f, 0f);
            Quaternion forwardToTarget = Quaternion.LookRotation(right, Vector3.up);

            return forwardToTarget * rightToForward;
        }
        public void reset()
        {
            removeSkeletons(new List<int>(beams.Keys));
        }

        public void setVisualizationEnabled(bool value)
        {
            enabled = value;
            if (!enabled)
            {
                //clean up
                reset();
            }
        }

    }
}
