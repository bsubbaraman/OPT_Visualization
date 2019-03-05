using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace OpenPTrack
{
	public class CentroidsManager : MonoBehaviour
	{
        public GameObject centroidObject;
        public Dictionary<int, GameObject> activeTracks = new Dictionary<int, GameObject>();
        public TfListener tfListener;
        public string ros_frame_id;
        public GameObject centroidsParent;
        public FacesManager faceManager;

		
        public CentroidReceiver centroidReceiver;
		// Use this for initialization
		void Start ()
		{		
		}
		
		// Update is called once per frame
		void Update ()
		{
			Dictionary<int, Vector3Msg> dataFromCentroidSub = centroidReceiver.getTrackData();

			//Data from skeletonSub
			//Dictionary<int, Vector3> dataFromSkeletonSubCentroid = skeletonSub.centroidPose;
			OptLogger.info("Received data from CentroidSub length: " + dataFromCentroidSub.Count);

            TransformMsg rosToArcoreTransform;
            if (ros_frame_id != null)
            {
                if (tfListener != null)
                {
                    try
                    {
                        rosToArcoreTransform = tfListener.lookupTransform(ros_frame_id, "world", Utils.getRosTime(0));
                    }
                    catch (TfListener.TransformException e)
                    {
                        OptLogger.warn(e.Message);
                        return;
                    }
                }
                else
                {
                    OptLogger.error("ros_frame_id is set but no tfListener is provided, cannot get transform");
                    rosToArcoreTransform = new TransformMsg();
                }
            }
            else
            {
                rosToArcoreTransform = new TransformMsg();
            }

			OptLogger.info("got transform, translation is "+rosToArcoreTransform.getTranslationVector3());

			foreach (KeyValuePair<int, Vector3Msg> track in dataFromCentroidSub)
			{
			    OptLogger.info("updating track");
				//int id = track.Key;
				//Vector3 poseInput = track.Value;

                
                Vector3 positionUnityArcore = Utils.rosToUnity(TfListener.transformPoint(rosToArcoreTransform, track.Value));

				//add any people who have joined the scene
				if (!activeTracks.ContainsKey(track.Key))
				{
                    OptLogger.info("creating centroid");
					Color color = new Color(
							UnityEngine.Random.Range(0f, 1f),
							UnityEngine.Random.Range(0f, 1f),
							UnityEngine.Random.Range(0f, 1f),
											1);

                    centroidObject.GetComponent<SetupCentroidSphere>().color = color;
                    GameObject newCentroid = Instantiate(centroidObject);
                    if (centroidsParent != null)
                        newCentroid.transform.parent = centroidsParent.transform;
                    OptLogger.info("Instantiated centroid");
                    newCentroid.transform.SetPositionAndRotation(  positionUnityArcore, new Quaternion());
                    OptLogger.info("pose set");
					newCentroid.name = "centroid_" + track.Key;
                    OptLogger.info("color set");
					activeTracks.Add(track.Key, newCentroid);
                    OptLogger.info("created centroid");
					//OptarLogger.info("I: Create centroid -> Parent: " + activeTracks[track.Key].transform.parent.name + " | Position: " + activeTracks[track.Key].transform.localPosition.ToString() + " | Id: " + track.Key);
				}
				else
				{
                    OptLogger.info("updating existing track");
					activeTracks[track.Key].transform.localPosition = positionUnityArcore;
                    OptLogger.info("updated");
					//OptarLogger.info("I: Update centroid  -> Parent: " + activeTracks[track.Key].transform.parent.name + " | Position: " + activeTracks[track.Key].transform.localPosition.ToString() + " | Id: " + track.Key);
				}
                OptLogger.info("centroid position is "+activeTracks[track.Key].transform.position);
			}

			//remove any people who are no longer present
			foreach (KeyValuePair<int, GameObject> kvp in activeTracks)
			{
				if (!dataFromCentroidSub.ContainsKey(kvp.Key))
				{
					if (activeTracks[kvp.Key])
					{
						Destroy(activeTracks[kvp.Key]);
						activeTracks.Remove(kvp.Key);
                    }
                    OptLogger.info("Removed centroid " + kvp.Key);
                }
			}

            if(faceManager!=null)
                applyFaces();
		}

        /**
         *  Removes all the currently displayed centroids
         */
        public void reset()
        {
            if (activeTracks.Count > 0)
            {
                foreach (KeyValuePair<int, GameObject> kvp in activeTracks)
                {
                    if (activeTracks[kvp.Key])
                    {
                        Destroy(activeTracks[kvp.Key]);
                        activeTracks.Remove(kvp.Key);
                    }

                    OptLogger.info("Destroyed centroid: " + kvp.Key);
                }
            }
        }

        public void setVisualizationEnabled(bool value)
		{
            enabled = value;
            if(!enabled)
            {
                //clean up
                reset();
            }
		}
		
        private void applyFaces()
        {

            Dictionary<int, Vector3Msg> centroidTracks = centroidReceiver.getTrackData();
            Dictionary<int, string> faceTracks = faceManager.getFaceTracks();

            OptLogger.info("applying faces");
            foreach (KeyValuePair<int, string> face_track in faceTracks)
            {
                Debug.Log(face_track.Value);
                if (centroidTracks.ContainsKey(face_track.Key))
                {
                    string face_name = face_track.Value;
                    Material faceMaterial = faceManager.getFaceMaterial(face_name);
                    OptLogger.info("faceMaterial for " + face_track.Key + " is " + (faceMaterial == null ? "null" : "not null"));
                    if(faceMaterial!=null)
                        activeTracks[face_track.Key].GetComponent<Renderer>().material = faceMaterial;

                }
            }

        }
    }

}