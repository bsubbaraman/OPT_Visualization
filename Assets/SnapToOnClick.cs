using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RosSharp.RosBridgeClient
{
    public class SnapToOnClick : MonoBehaviour
    {
        private Camera main;
        public PoseStampedSubscriber sub;
    // Use this for initialization
    void Start()
        {
            main = GameObject.Find("Main Camera").GetComponent<Camera>();
            sub = GameObject.Find("RosConnector").GetComponent<PoseStampedSubscriber>();
        }


        // Update is called once per frame
        void Update()
        {
            transform.position = sub.PublishedTransforms[0].position;
            transform.rotation = sub.PublishedTransforms[0].rotation;
        }

        private void OnMouseOver()
        {
            if (Input.GetMouseButtonDown(0))
            {
                main.transform.position = transform.position;
                main.transform.rotation = transform.rotation;

            }
        }
    }
}