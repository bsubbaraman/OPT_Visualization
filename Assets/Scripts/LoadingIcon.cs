using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingIcon : MonoBehaviour {
    public GameObject cube;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float increment = Remap(Mathf.Sin(Time.time), -1, 1, 0.1f, 1f );
        Quaternion rot = cube.transform.rotation * Quaternion.Euler(new Vector3(increment, increment, increment));
        cube.transform.rotation = rot;

	}


    float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}


