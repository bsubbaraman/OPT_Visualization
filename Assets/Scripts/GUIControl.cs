using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace RosSharp.RosBridgeClient
{

    public class GUIControl : MonoBehaviour
    {
        public Button m_CentroidButton, m_SkeletonButton, m_ObjectButton, m_ImageButton, m_SnapToButton;
        public Camera main;
        public GameObject theConnector, theController, theImage;
        public Visualization v;
        // Use this for initialization
        void Start()
        {
            v = theController.GetComponent<Visualization>();
            m_CentroidButton.onClick.AddListener(() => TaskOnClick(m_CentroidButton));
            m_SkeletonButton.onClick.AddListener(() => TaskOnClick(m_SkeletonButton));
            m_ObjectButton.onClick.AddListener(() => TaskOnClick(m_ObjectButton));
            m_ImageButton.onClick.AddListener(() => TaskOnClick(m_ImageButton));

            //position the image in bottom left of screen
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
                    break;
                case "SkeletonsButton":
                    v.skeletonView = !v.skeletonView;
                    break;
                case "ObjectsButton":
                    v.objectView = !v.objectView;
                    break;
                case "ImageButton":
                    theConnector.GetComponent<ImageSubscriber>().enabled = !theConnector.GetComponent<ImageSubscriber>().enabled;
                    theImage.SetActive(!theImage.activeSelf);
                    break;
            }
        }
        // Update is called once per frame
        void Update()
        {
            //theImage.transform.position = main.ViewportToWorldPoint(new Vector3(.5f, .5f, 0));
        }
    }
}
