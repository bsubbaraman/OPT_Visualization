using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Camera main;

    //zoom
    float sensitivity = 10f;

    //drag movement
    public float dragSpeed = 2f;
    private Vector3 dragOrigin;

    //orbit movement
    public float orbitSensitivity = 2f;
    private float xRotate;
    private float yRotate;

    // Update is called once per frame
    void Update()
    {
        Zoom();
        Drag();
        Orbit();
    }

    void Zoom(){
        //float fov = main.fieldOfView;
        //float z = transform.position.z;
        //z += Input.GetAxis("Mouse ScrollWheel");// * sensitivity;

        //Vector3 zoom = z * transform.forward;
        //fov = Mathf.Clamp(fov, minFov, maxFov);

        //transform.position += zoom;//new Vector3(transform.position.x, transform.position.y, z);
        //main.fieldOfView = fov;

        transform.position += transform.forward * Input.GetAxis("Mouse ScrollWheel");
    }

    void Drag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
            return;
        }

        if (!Input.GetMouseButton(0)) return;

        Vector3 pos = main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
        Vector3 move = new Vector3(pos.x * dragSpeed, 0, pos.y * dragSpeed);

        transform.Translate(move, Space.World);
    }

    void Orbit(){
        //if (Input.GetMouseButtonDown(1))
        //{
        //    Debug.Log("r click");
        //    //xRotate = Input.GetAxis("Mouse X");
        //    //yRotate = Input.GetAxis("Mouse Y");
        //    Debug.Log(xRotate);
        //    return;
        //}

        //if (!Input.GetMouseButton(1)) return;

        if (Input.GetMouseButton(1))
        {


            xRotate = Input.GetAxis("Mouse X");
            yRotate = Input.GetAxis("Mouse Y");
            //xRotate = Mathf.Lerp(xRotate, 0, lerpRate);
            //yRotate = Mathf.Lerp(yRotate, 0, lerpRate);

            Debug.Log(xRotate);

            transform.eulerAngles += new Vector3(yRotate, xRotate, 0);
        }
    }
}
