using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RosSharp.RosBridgeClient
{
    public class CameraMovement : MonoBehaviour
    {
        public Camera main;
        public GUIControl guiControl;


        //drag movement
        public float dragSpeed = 1f;
        private Vector3 dragOrigin;

        //orbit movement
        public float orbitSensitivity = 1f;
        private float xRotate;
        private float yRotate;

        // Update is called once per frame
        void Update()
        {
            Zoom();
            Drag();
            Orbit();
        }

        void Zoom()
        {
            transform.position += transform.forward * Input.GetAxis("Mouse ScrollWheel");
        }

        void Drag()
        {
            if (!guiControl.healthPopup){
                if (Input.GetMouseButtonDown(0))
                {
                    dragOrigin = Input.mousePosition;
                    return;
                }

                if (!Input.GetMouseButton(0)) return;

                Vector3 pos = main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
                Vector3 move = new Vector3(pos.x * dragSpeed, pos.y * dragSpeed, 0);

                transform.Translate(move, Space.World);
            }
        }

        void Orbit()
        {
            if (Input.GetMouseButton(1))
            {
                xRotate = orbitSensitivity * Input.GetAxis("Mouse X");
                yRotate = orbitSensitivity * Input.GetAxis("Mouse Y");
                transform.eulerAngles += new Vector3(-yRotate, xRotate, 0);
            }
        }
    }
}