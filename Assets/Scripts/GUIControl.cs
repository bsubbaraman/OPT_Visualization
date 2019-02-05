﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace RosSharp.RosBridgeClient
{

    public class GUIControl : MonoBehaviour
    {
        public Button m_CentroidButton, m_SkeletonButton, m_ObjectButton, m_ShowImageButton, m_SnapToButton, m_LabelButton, m_GetSnapButton;
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
            m_ShowImageButton.onClick.AddListener(() => TaskOnClick(m_ShowImageButton));
            m_SnapToButton.onClick.AddListener(() => TaskOnClick(m_SnapToButton));
            m_LabelButton.onClick.AddListener(() => TaskOnClick(m_LabelButton));
            m_GetSnapButton.onClick.AddListener(() => TaskOnClick(m_GetSnapButton));

            // change panel components based on window size

            RectTransform rT = Panel.GetComponent<RectTransform>();
            //rT.sizeDelta = new Vector2(rT.sizeDelta.x, Screen.height - 20f);
            rT.sizeDelta = new Vector2(Screen.width/8f, Screen.height-20f);
            rT = m_CentroidButton.GetComponent<RectTransform>();
            rT.localPosition = new Vector2(rT.localPosition.x, -30f);
            rT.sizeDelta = new Vector2(Screen.width / 8f - 20f, Screen.height/20f);
            rT = m_SkeletonButton.GetComponent<RectTransform>();
            rT.localPosition = new Vector2(rT.localPosition.x, -30f - (Screen.height / 20f));
            rT.sizeDelta = new Vector2(Screen.width / 8f - 20f, Screen.height / 20f);
            rT = m_ObjectButton.GetComponent<RectTransform>();
            rT.localPosition = new Vector2(rT.localPosition.x, -30f - 2*(Screen.height / 20f));
            rT.sizeDelta = new Vector2(Screen.width / 8f - 20f, Screen.height / 20f);
            rT = m_ShowImageButton.GetComponent<RectTransform>();
            rT.localPosition = new Vector2(rT.localPosition.x, -30f - 3*(Screen.height / 20f));
            rT.sizeDelta = new Vector2(Screen.width / 8f - 20f, Screen.height / 20f);
            rT = m_GetSnapButton.GetComponent<RectTransform>();
            rT.localPosition = new Vector2(rT.localPosition.x, -30f - 4 * (Screen.height / 20f));
            rT.sizeDelta = new Vector2(Screen.width / 8f - 20f, Screen.height / 20f);
            rT = m_SnapToButton.GetComponent<RectTransform>();
            rT.localPosition = new Vector2(rT.localPosition.x, -30f - 5*(Screen.height / 20f));
            rT.sizeDelta = new Vector2(Screen.width / 8f - 20f, Screen.height / 20f);
            rT = m_LabelButton.GetComponent<RectTransform>();
            rT.localPosition = new Vector2(rT.localPosition.x, -30f - 5*(Screen.height / 20f));
            rT.sizeDelta = new Vector2(Screen.width / 8f - 20f, Screen.height / 20f);
        }

        void TaskOnClick(Button b)
        {
            ColorBlock cb = b.colors;
            cb.normalColor = b.colors.normalColor == Color.green ? Color.white : Color.green;
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
                case "ShowImageButton":
                    //theConnector.GetComponent<ImageSubscriber>().enabled = !theConnector.GetComponent<ImageSubscriber>().enabled;
                    theImage.SetActive(!theImage.activeSelf);
                    iS.pulse = true;
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
