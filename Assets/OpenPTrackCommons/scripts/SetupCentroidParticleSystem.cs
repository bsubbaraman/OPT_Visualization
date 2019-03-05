using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenPTrack;

namespace OpenPTrack
{
    [RequireComponent(typeof(ParticleSystem))]
    public class SetupCentroidParticleSystem : MonoBehaviour
    {
        private void Awake()
        {
            OptLogger.info("Setting up particle system");
            Color color = transform.parent.gameObject.GetComponent<SetupCentroidSphere>().color;
            ParticleSystem particleSystem = GetComponent<ParticleSystem>();
            var mainPartSyst = particleSystem.main;
            mainPartSyst.startColor = color;

            var emissionPartSyst = particleSystem.emission;
            emissionPartSyst.enabled = true;
            emissionPartSyst.rateOverTime = 0f;
            emissionPartSyst.rateOverDistance = 10f;

            var shapePartSyst = particleSystem.shape;
            shapePartSyst.enabled = true;
            shapePartSyst.shapeType = ParticleSystemShapeType.Sphere;
            shapePartSyst.radius = 0.01f;
            shapePartSyst.radiusThickness = 1f;


            Gradient gradient = new Gradient();
            GradientColorKey[] colorKey;
            GradientAlphaKey[] alphaKey;

            // Populate the color keys at the relative time 0 and 1 (0 and 100%)
            colorKey = new GradientColorKey[2];
            colorKey[0].color = color;
            colorKey[0].time = 0.0f;
            colorKey[1].color = color;
            colorKey[1].time = 1.0f;

            // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
            alphaKey = new GradientAlphaKey[2];
            alphaKey[0].alpha = 1.0f;
            alphaKey[0].time = 0.782f;
            alphaKey[1].alpha = 0.0f;
            alphaKey[1].time = 1.0f;
            gradient.SetKeys(colorKey, alphaKey);

            var colorOverTimePartSyst = particleSystem.colorOverLifetime;
            colorOverTimePartSyst.enabled = true;
            colorOverTimePartSyst.color = gradient;

            var sizeOverTimePartSyst = particleSystem.sizeOverLifetime;
            sizeOverTimePartSyst.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0.0f, 1.0f);
            curve.AddKey(1.0f, 0.0f);
            sizeOverTimePartSyst.size = new ParticleSystem.MinMaxCurve(1.5f, curve);

            OptLogger.info("Particle system set up");
        }

        private void Update()
        {
            OptLogger.info("Particle system position is " + transform.position);
        }
    }
}