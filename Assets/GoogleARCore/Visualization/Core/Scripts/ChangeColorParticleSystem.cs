using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColorParticleSystem : MonoBehaviour {

    private ParticleSystem ps;
    private Color color;

    // Use this for initialization
    void Start () {
        ps = GetComponent<ParticleSystem>();
        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(Color.blue, 0.0f), new GradientColorKey(Color.red, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) });

        col.color = grad;
    }

    public void SetColor(Color input){
        color = input;
    }
}