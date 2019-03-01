using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//namespace OpenPTrack
namespace RosSharp.RosBridgeClient
{
    public class SnapToOnClick : MonoBehaviour
    {
        private Camera main;
        public PoseStampedSubscriber sub;

        //private TfListener TfListener;
    // Use this for initialization
    void Start()
        {
            main = GameObject.Find("Main Camera").GetComponent<Camera>();
            //TfListener = GameObject.Find("RosConnector").GetComponent<TfListener>();
            sub = GameObject.Find("RosConnector").GetComponent<PoseStampedSubscriber>();
        }


        // Update is called once per frame
        void Update()
        {
            //TfListener.lookupTransform("");
            transform.position = sub.PublishedTransforms[0].position;
            transform.rotation = sub.PublishedTransforms[0].rotation;
        }

        private void OnMouseOver()
        {
            Debug.Log("OVer");
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