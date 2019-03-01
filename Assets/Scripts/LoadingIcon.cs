using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingIcon : MonoBehaviour {
    public GameObject cube;

    Quaternion startRot;
	// Use this for initialization
	void Start () {

        startRot = cube.transform.rotation;
    }
	
	// Update is called once per frame
	void Update () {
        //float increment = Remap(Mathf.Sin(2 * Mathf.PI * Time.time/5f), -1f, 1f, 0f, 1f );
        //Quaternion rot = cube.transform.rotation * Quaternion.Euler(new Vector3(increment, increment, increment));
        //cube.transform.rotation = rot;

        //Quaternion currentRot = cube.transform.rotation;
        ////Quaternion targetRot = Quaternion.Euler(new Vector3(, -180f, 355f));

        //cube.transform.rotation = Quaternion.Lerp(startRot, targetRot, Time.time/5f);
        //Debug.Log(Time.time / 5f);

    }


    float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}


