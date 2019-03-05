

using UnityEngine;
using System.Collections.Generic;

namespace OpenPTrack
{
    public class CentroidReceiver : MonoBehaviour
    {
        private OpenPTrack.TrackArrayMsg trackArray;
        private bool isMessageReceived;
        private Dictionary<int, Vector3Msg> processedTrackData = new Dictionary<int, Vector3Msg>();
        private GenericSubscriber<OpenPTrack.TrackArrayMsg> subscriber;
        private System.Threading.Mutex mutex = new System.Threading.Mutex();
        private const int MUTEX_TIMEOUT_MILLIS = 1000;
		public string topicName;
		public RosSharp.RosBridgeClient.RosConnector rosConnector;
        protected void Start()
        {           
            subscriber = new GenericSubscriber<TrackArrayMsg>(rosConnector,topicName);
            subscriber.addCallback(ReceiveMessage);
        }


        protected void ReceiveMessage(OpenPTrack.TrackArrayMsg message)
        {
            OptLogger.info("received centroid message");
            trackArray = message;
            isMessageReceived = true;


            OptLogger.info("processing " + trackArray.tracks.Length + " centroids");
            bool gotResource = mutex.WaitOne(MUTEX_TIMEOUT_MILLIS);
            if (!gotResource)
            {
                OptLogger.warn("Failed to get mutex, dropping message from " + topicName);
                return;
            }
            processedTrackData.Clear();
            foreach (OpenPTrack.TrackMsg track in trackArray.tracks)
            {
                if (!processedTrackData.ContainsKey(track.id))
                {
                    processedTrackData.Add(track.id, new Vector3Msg(track.x, track.y, track.height));
                }
                else
                {
                    processedTrackData[track.id] = new Vector3Msg(track.x, track.y, track.height);
                }
            }
            OptLogger.info("we now have " + processedTrackData.Count + " centroids");
            mutex.ReleaseMutex();
        }

        public Dictionary<int, Vector3Msg> getTrackData()
        {
            Dictionary<int, Vector3Msg> ret = new Dictionary<int, Vector3Msg>();
            bool gotResource = mutex.WaitOne(MUTEX_TIMEOUT_MILLIS);
            if (!gotResource)
            {
                OptLogger.warn("Failed to get mutex, unable to provide skeleton poses");
                return ret;//empty
            }
            if (trackArray != null)
            {
                //make a deep copy
                foreach (OpenPTrack.TrackMsg track in trackArray.tracks)
                    ret.Add(track.id, new Vector3Msg(track.x, track.y, track.height));
            }
            mutex.ReleaseMutex();
            return ret;
        }
    }
}