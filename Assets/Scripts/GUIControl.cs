using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace RosSharp.RosBridgeClient
{

    public class GUIControl : MonoBehaviour
    {
        public Button m_CentroidButton, m_SkeletonButton, m_ObjectButton, m_FacesButton, m_ShowImageButton, m_SnapToButton, m_LabelButton, m_GetSnapButton, m_SystemHealth;
        public Camera main;
        public GameObject theConnector, theController, theImage, cameraRepresentation, Panel, PanelText;
        public Visualization v;
        public ImageSubscriber iS;
        public TFSubscriber s;

        public Color activeColour;
        public bool healthPopup;
        private Rect windowRect = new Rect(Screen.width / 2 - (Screen.width / 4 / 2), Screen.height / 2 - Screen.height / 4 / 2, Screen.width / 3, Screen.height / 3);

        public RosConnector rosConnector;
        public CentroidSubscriber centroidSub;
        public SkeletonSubscriber skeletonSub;
        public ObjectsSubscriber objectSub;
        public UDPSubscriber_Pose poseSub;

        // Use this for initialization
        void Start()
        {
            v = theController.GetComponent<Visualization>();
            iS = theConnector.GetComponent<ImageSubscriber>();
            m_CentroidButton.onClick.AddListener(() => TaskOnClick(m_CentroidButton));
            m_SkeletonButton.onClick.AddListener(() => TaskOnClick(m_SkeletonButton));
            m_ObjectButton.onClick.AddListener(() => TaskOnClick(m_ObjectButton));
            m_FacesButton.onClick.AddListener(() => TaskOnClick(m_FacesButton));
            m_ShowImageButton.onClick.AddListener(() => TaskOnClick(m_ShowImageButton));
            m_SnapToButton.onClick.AddListener(() => TaskOnClick(m_SnapToButton));
            m_LabelButton.onClick.AddListener(() => TaskOnClick(m_LabelButton));
            m_GetSnapButton.onClick.AddListener(() => TaskOnClick(m_GetSnapButton));
            m_SystemHealth.onClick.AddListener(() => TaskOnClick(m_SystemHealth));
            // change panel components based on window size


            RectTransform rT = Panel.GetComponent<RectTransform>();
            //rT.sizeDelta = new Vector2(rT.sizeDelta.x, Screen.height - 20f);
            rT.sizeDelta = new Vector2(Screen.width/8f, Screen.height-20f);

            rT = m_CentroidButton.GetComponent<RectTransform>();
            rT.localPosition = new Vector2(rT.localPosition.x, -50f);
            rT.sizeDelta = new Vector2(Screen.width / 8f - 20f, Screen.height/25f);

            rT = m_SkeletonButton.GetComponent<RectTransform>();
            rT.localPosition = new Vector2(rT.localPosition.x, -50f - (Screen.height / 25f));
            rT.sizeDelta = new Vector2(Screen.width / 8f - 20f, Screen.height / 25f);

            rT = m_ObjectButton.GetComponent<RectTransform>();
            rT.localPosition = new Vector2(rT.localPosition.x, -50f - 2*(Screen.height / 25f));
            rT.sizeDelta = new Vector2(Screen.width / 8f - 20f, Screen.height / 25f);

            rT = m_FacesButton.GetComponent<RectTransform>();
            rT.localPosition = new Vector2(rT.localPosition.x, -50f - 3 * (Screen.height / 25f));
            rT.sizeDelta = new Vector2(Screen.width / 8f - 20f, Screen.height / 25f);

            rT = m_ShowImageButton.GetComponent<RectTransform>();
            rT.localPosition = new Vector2(rT.localPosition.x, -50f - 4*(Screen.height / 25f));
            rT.sizeDelta = new Vector2(Screen.width / 8f - 20f, Screen.height / 25f);

            rT = m_GetSnapButton.GetComponent<RectTransform>();
            rT.localPosition = new Vector2(rT.localPosition.x, -50f - 5 * (Screen.height / 25f));
            rT.sizeDelta = new Vector2(Screen.width / 8f - 20f, Screen.height / 25f);

            rT = m_SnapToButton.GetComponent<RectTransform>();
            rT.localPosition = new Vector2(rT.localPosition.x, -50f - 6*(Screen.height / 25f));
            rT.sizeDelta = new Vector2(Screen.width / 8f - 20f, Screen.height / 25f);

            rT = m_LabelButton.GetComponent<RectTransform>();
            rT.localPosition = new Vector2(rT.localPosition.x, -50f - 7*(Screen.height / 25f));
            rT.sizeDelta = new Vector2(Screen.width / 8f - 20f, Screen.height / 25f);

            rT = m_SystemHealth.GetComponent<RectTransform>();
            rT.localPosition = new Vector2(rT.localPosition.x, -50f - 8 * (Screen.height / 25f));
            rT.sizeDelta = new Vector2(Screen.width / 8f - 20f, Screen.height / 25f);

            rT = PanelText.GetComponent<RectTransform>();
            rT.sizeDelta = new Vector2(Screen.width / 8f, Screen.height - 20f);

        }

        void TaskOnClick(Button b)
        {
            ColorBlock cb = b.colors;
            //cb.normalColor = b.colors.normalColor == Color.green ? Color.white : Color.green;
            //cb.normalColor = aC;
            cb.normalColor = b.colors.normalColor == activeColour ? Color.white : activeColour;
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
                case "FacesButton":
                    if (v.facesView == false)
                    {
                        if (v.labelView == false)
                        {
                            TaskOnClick(m_LabelButton);
                        }
                    }
                    v.facesView = !v.facesView;
                    break;
                case "ShowImageButton":
                    //theConnector.GetComponent<ImageSubscriber>().enabled = !theConnector.GetComponent<ImageSubscriber>().enabled;
                    theImage.SetActive(!theImage.activeSelf);
                    iS.pulse = true;
                    break;
                case "GetSnapButton":
                    iS.pulse = true;
                    cb.normalColor = Color.white;
                    cb.highlightedColor = cb.normalColor;
                    b.colors = cb;
                    break;
                case "SnapToCamView":
                    //Quaternion orient = Quaternion.LookRotation(s.cameraRot.eulerAngles, Vector3.up);
                    //main.transform.position = s.cameraPos;
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
                case "SystemHealth":
                    healthPopup = !healthPopup;
                    break;
            }
        }


        private GUIStyle systemHealthStyle = new GUIStyle();
        public Font font;

        //public Font font;
        private void OnGUI()
        {
            systemHealthStyle.normal.textColor = Color.white;
            systemHealthStyle.fontSize = 30;
            if (healthPopup){
                windowRect = GUI.Window(0, windowRect, DoMyWindow, "System Health");
            }
        }

        void DoMyWindow(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 50));
            GUI.Label(new Rect(10, 20, 200, 20), "IP Address", systemHealthStyle);
            GUI.Label(new Rect(Screen.width / 3 - 100, 20, 100, 20), rosConnector.RosBridgeServerUrl, systemHealthStyle);
            GUI.Label(new Rect(10, 50, 200, 20), "Topic Name", systemHealthStyle);
            GUI.Label(new Rect(Screen.width / 3 - 80, 50, 50, 20), "f (hz)", systemHealthStyle);
            GUI.Label(new Rect(10, 80, 200, 20), "/tracker/tracks_smoothed:", systemHealthStyle);
            GUI.Label(new Rect(Screen.width / 3 - 80, 80, 50, 20), centroidSub.centroidRate.ToString("F1"), systemHealthStyle);
            GUI.Label(new Rect(10, 110, 200, 20), "/tracker/skeleton_tracks:", systemHealthStyle);
            GUI.Label(new Rect(Screen.width / 3 - 80, 110, 50, 20), skeletonSub.skeletonRate.ToString("F1"), systemHealthStyle);
            GUI.Label(new Rect(10, 140, 200, 20), "/tracker/object_tracks:", systemHealthStyle);
            GUI.Label(new Rect(Screen.width / 3 - 80, 140, 50, 20), objectSub.objectRate.ToString("F1"), systemHealthStyle);

            //GUI.Label(new Rect(10, 140, 200, 20), "UDP: Recognized Poses", systemHealthStyle);
            //GUI.Label(new Rect(Screen.width / 3 - 80, 140, 50, 20), poseSub.recognizedPoseRate.ToString("F1"), systemHealthStyle);


            //IPEnter = GUI.TextField(new Rect(20, 40, Screen.width / 4 - 40, 20), IPEnter);
            //if (GUI.Button(new Rect(windowRect.width / 2 - (Screen.width / 4 - 60) / 2, windowRect.height / 2 - 10, 150, 40), "Set New IP"))
            //if (GUI.Button(new Rect(50, 50, 150, 40), "Set New IP"))
            //{
            //    print("Got a click");
            //}
        }
        // Update is called once per frame
        void Update()
        {
            //theImage.transform.position = main.ViewportToWorldPoint(new Vector3(.5f, .5f, 0));
        }
    }
}
