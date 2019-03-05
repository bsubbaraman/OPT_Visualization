using System.Collections;
using System.Collections.Generic;
using OpenPTrack;
using UnityEngine;

namespace OpenPTrack
//namespace RosSharp.RosBridgeClient
{
    public class SnapToOnClick : MonoBehaviour
    {
        private Camera main;
        //public PoseStampedSubscriber sub;

        public RosSharp.RosBridgeClient.RosConnector rosConnector;
        public TfListener listener;

        private GenericSubscriber<OptarPoseStampedMsg> poseSub;
        private OptarPoseStampedMsg transformedPose;
    // Use this for initialization
    void Start()
        {
            main = GameObject.Find("Main Camera").GetComponent<Camera>();

            poseSub = new GenericSubscriber<OptarPoseStampedMsg>(rosConnector, "/optar/arcore_pose");
            poseSub.addCallback(ReceiveMessage);


            //TfListener = GameObject.Find("RosConnector").GetComponent<TfListener>();
            //sub = GameObject.Find("RosConnector").GetComponent<PoseStampedSubscriber>();
        }


        // Update is called once per frame
        void Update()
        {
            //TfListener.lookupTransform("");
            //transform.position = sub.PublishedTransforms[0].position;
            //transform.rotation = sub.PublishedTransforms[0].rotation;
            try{
                transform.position = new Vector3(transformedPose.pose.position.x, transformedPose.pose.position.y, transformedPose.pose.position.z);
                transform.rotation = new Quaternion(transformedPose.pose.orientation.x, transformedPose.pose.orientation.y, transformedPose.pose.orientation.z, transformedPose.pose.orientation.w);
            }
            catch{
                Debug.Log("didn't move/orient phone");
            }

        }

        private void ReceiveMessage(OptarPoseStampedMsg message){
            TransformMsg transformMsg;
            Debug.Log("Got Message");
            try{
                transformMsg = listener.lookupTransform(message.header.frame_id, "/world", Utils.getRosTime(0));
                //Instantiate(MobilePhonePrefab, transformMsg.getTranslationVector3(), transformMsg.getOrientation());
            }
            catch (TfListener.TransformException e){
                Debug.Log(e);
                Debug.Log("Got Message BUT problem");
                return;
            }

            transformedPose = TfListener.transformPose(transformMsg, message, "/world");

        }

        private void OnMouseOver()
        {
            Debug.Log("Over");
            if (Input.GetMouseButtonDown(0))
            {
                main.transform.position = transform.position;
                main.transform.rotation = transform.rotation * Quaternion.Euler(90f,0f,0f);

            }
        }

        Quaternion YLookRotation(Vector3 up)
        {
            Quaternion upToForward = Quaternion.Euler(90f, 0f, 0f);
            Quaternion forwardToTarget = Quaternion.LookRotation(up, Vector3.up);

            return forwardToTarget * upToForward;
        }

        Quaternion XLookRotation(Vector3 right)
        {
            Quaternion rightToForward = Quaternion.Euler(0f, -90f, 0f);
            Quaternion forwardToTarget = Quaternion.LookRotation(right, Vector3.up);

            return forwardToTarget * rightToForward;
        }
    }
}