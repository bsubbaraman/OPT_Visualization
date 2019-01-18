using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace RosSharp.RosBridgeClient
{

    public class GUIControl : MonoBehaviour
    {
        public Button m_CentroidButton, m_SkeletonButton, m_ObjectButton, m_ImageButton, m_SnapToButton, m_LabelButton;
        public Camera main;
        public GameObject theConnector, theController, theImage, cameraRepresentation, Panel;
        public Visualization v;
        public ImageSubscriber iS;
        public TFSubscriber s;

        // Use this for initialization
        void Start()
        {
            v = theController.GetComponent<Visualization>();
            iS = theConnector.GetComponent<ImageSubscriber>();
            m_CentroidButton.onClick.AddListener(() => TaskOnClick(m_CentroidButton));
            m_SkeletonButton.onClick.AddListener(() => TaskOnClick(m_SkeletonButton));
            m_ObjectButton.onClick.AddListener(() => TaskOnClick(m_ObjectButton));
            m_ImageButton.onClick.AddListener(() => TaskOnClick(m_ImageButton));
            m_SnapToButton.onClick.AddListener(() => TaskOnClick(m_SnapToButton));
            m_LabelButton.onClick.AddListener(() => TaskOnClick(m_LabelButton));

            RectTransform rT = Panel.GetComponent<RectTransform>();
            rT.sizeDelta = new Vector2(rT.sizeDelta.x, Screen.height - 20f);
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
                    //theConnector.GetComponent<ImageSubscriber>().enabled = !theConnector.GetComponent<ImageSubscriber>().enabled;
                    theImage.SetActive(!theImage.activeSelf);
                    //iS.pulse = true;
                    break;
                case "SnapToCamView":
                    //Quaternion orient = Quaternion.LookRotation(s.cameraRot.eulerAngles, Vector3.up);
                    main.transform.position = s.cameraPos;
                    //main.transform.rotation = s.cameraRot;
                    main.transform.rotation = cameraRepresentation.transform.rotation;
                    main.transform.Rotate(-90f,0f,0f, Space.Self);
                    main.transform.Rotate(0f, 0f, 90f, Space.Self);
                    cb.normalColor = Color.white;
                    cb.highlightedColor = cb.normalColor;
                    b.colors = cb;
                    break;
                case "LabelsButton":
                    v.ToggleLabels();
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
