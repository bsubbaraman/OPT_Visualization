using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenPTrack
{
    public class FacesManager : MonoBehaviour
    {

        private Dictionary<string, Material> faceMaterials = new Dictionary<string, Material>();
        private System.Threading.Mutex mutex = new System.Threading.Mutex();
        private const int MUTEX_TIMEOUT_MILLIS = 1000;

        public Dictionary<int, string> faceData = new Dictionary<int, string>();
        public List<Texture2D> faceImages = new List<Texture2D>(); // drag & drop images into inspector here, named as they are in the OpenPTrack System
        public OpenPTrack.GenericSubscriber<OpenPTrack.TrackArrayMsg> recognizedFacesSub; //drop face subscriber here from the RosConnector
        public RosSharp.RosBridgeClient.RosConnector rosConnector; //drop the visualization script here
        public string topicName;
        // Use this for initialization
        void Start()
        {
            recognizedFacesSub = new OpenPTrack.GenericSubscriber<OpenPTrack.TrackArrayMsg>(rosConnector, topicName);
            recognizedFacesSub.addCallback(receiveMessage);
            // create materials from each face image
            foreach (Texture2D face in faceImages)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.SetTexture("_MainTex", face);
                faceMaterials.Add(face.name, mat);
            }
        }

        private void receiveMessage(OpenPTrack.TrackArrayMsg message)
        {
            bool gotResource = mutex.WaitOne(MUTEX_TIMEOUT_MILLIS);
            if (!gotResource)
            {
                OptLogger.warn("Failed to get mutex, dropping message from " + topicName);
                return;
            }

            faceData.Clear();
            foreach (OpenPTrack.TrackMsg track in message.tracks)
            {
                string face_name = track.face_name;
                if (!string.IsNullOrEmpty(face_name))
                {
                    if (!faceData.ContainsKey(track.id))
                    {
                        faceData.Add(track.id, face_name);
                        OptLogger.info("Added face data for " + face_name);
                    }
                }
                else
                {
                    // do i need this?  should just stay the same
                    //faceData[track.id] = face_name;
                }

            }

            mutex.ReleaseMutex();
        }

        public Dictionary<int, string> getFaceTracks()
        {

            bool gotResource = mutex.WaitOne(MUTEX_TIMEOUT_MILLIS);
            if (!gotResource)
            {
                OptLogger.warn("Failed to get mutex, cannot get face track data received from " + topicName);
                return new Dictionary<int, string>();
            }

            //copy it
            Dictionary<int, string> ret = new Dictionary<int, string>(faceData);

            mutex.ReleaseMutex();
            return ret;
        }

        public Material getFaceMaterial(string faceId)
        {
            if (faceMaterials.ContainsKey(faceId))
                return faceMaterials[faceId];
            else
                return null;
        }
    }
}
