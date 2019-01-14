using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace RosSharp.RosBridgeClient
{

    public class GUIControl : MonoBehaviour
    {
        public Button m_CentroidButton, m_SkeletonButton, m_ObjectButton;
        public GameObject theController;
        public Visualization v;
        // Use this for initialization
        void Start()
        {
            v = theController.GetComponent<Visualization>();
            m_CentroidButton.onClick.AddListener(() => TaskOnClick(m_CentroidButton));
            m_SkeletonButton.onClick.AddListener(() => TaskOnClick(m_SkeletonButton));
            m_ObjectButton.onClick.AddListener(() => TaskOnClick(m_ObjectButton));
        }

        void TaskOnClick(Button b)
        {
            ColorBlock cb = b.colors;
            if (b.colors.normalColor == Color.green)
            {
                cb.normalColor = Color.white;
            }
            else
            {
                cb.normalColor = Color.green;
            }
            cb.highlightedColor = cb.normalColor;
            b.colors = cb;
            // update viewmode for viz
            switch (b.name)
            {
                case "CentroidsButton":
                    v.centroidView = !v.centroidView;
                    //if (v.centroidView == false){
                    //    v.RemoveAllCentroids();
                    //}
                    break;
                case "SkeletonsButton":
                    v.skeletonView = !v.skeletonView;
                    //if (v.skeletonView == false){
                    //    v.RemoveAllSkeletons();
                    //}
                    break;
                case "ObjectsButton":
                    v.objectView = !v.objectView;
                    //if (v.objectView == false){
                    //    v.RemoveAllObjects();
                    //}

                    break;
            }
        }
        // Update is called once per frame
        void Update()
        {

        }
    }
}
