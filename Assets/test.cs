using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour {
    private Animator a;
	// Use this for initialization
	void Start () {
        a = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
        Transform hip = a.GetBoneTransform(HumanBodyBones.Hips).GetChild(1);
        Transform foot = a.GetBoneTransform(HumanBodyBones.LeftFoot);
        Debug.Log(Vector3.Distance(hip.position, foot.position));
    }
}
