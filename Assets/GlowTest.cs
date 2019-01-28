using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MK.Glow;
public class GlowTest : MonoBehaviour {
    public GameObject p;
    private Material m;
	// Use this for initialization
	void Start () {
        m = p.GetComponent<Renderer>().material;
    }
	
	// Update is called once per frame
	void Update () {
        float gp = m.GetFloat("_MKGlowPower");
        gp = gp + 0.01f;
        m.SetFloat("_MKGlowPower", gp);


	}
}
