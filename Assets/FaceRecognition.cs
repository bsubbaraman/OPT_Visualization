using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceRecognition : MonoBehaviour {

    public List<Texture2D> faceImages = new List<Texture2D>(); // drag & drop images into inspector here, named as they are in the OpenPTrack System
    private Dictionary<string, Material> faceMaterials = new Dictionary<string, Material>();
    public RosSharp.RosBridgeClient.FaceSubscriber recognizedFacesSub; //drop face subscriber here from the RosConnector
    public RosSharp.RosBridgeClient.Visualization visualization; //drop the visualization script here
    // Use this for initialization
    void Start () {
        // create materials from each face image
        foreach (Texture2D face in faceImages)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.SetTexture("_MainTex", face);
            faceMaterials.Add(face.name, mat);
        }
    }
	
	// Update is called once per frame
	void Update () {
        Dictionary<int, string> dataFromFaceSub = recognizedFacesSub.faceData;
        foreach (KeyValuePair<int, string> face_track in dataFromFaceSub)
        {
            if (visualization.activeTracks.ContainsKey(face_track.Key))
            {
                string face_name = face_track.Value;
                visualization.activeTracks[face_track.Key].GetComponent<Renderer>().material = faceMaterials[face_name];

            }
        }
    }
}
