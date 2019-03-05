using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenPTrack
{
    [RequireComponent(typeof(Renderer))]
    public class SetupCentroidSphere : MonoBehaviour
    {
        public Color color;
        void Awake()
        {
            GetComponent<Renderer>().material.color = color;
        }
    }
}