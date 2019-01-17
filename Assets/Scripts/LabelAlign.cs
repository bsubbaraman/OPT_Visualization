using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabelAlign : MonoBehaviour {
    /// <summary>
    /// Camera label always needs to appear
    /// </summary>
	// Use this for initialization

    public Camera main;
	void Start () {
        Transform camLabel = this.transform;
        Vector3 camR = camLabel.parent.rotation.eulerAngles;
        camLabel.localRotation = Quaternion.Euler(-camR);

        camLabel.Translate(.1f * Vector3.up);
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 v = main.transform.position - transform.position;
        v.x = v.z = 0.0f;
        transform.LookAt(main.transform.position - v);
        transform.Rotate(0, 180, 0);
    }
}
