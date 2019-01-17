using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class MakeGrid : MonoBehaviour
{
    public GameObject WhitePlane;
    public GameObject BlackPlane;
    public Transform ParentGrid;
    public int width;
    public int height ;
    private GameObject[,] grid = new GameObject[100, 100];
    private GameObject[,] gridBlack = new GameObject[100, 100];

    private void Awake()
    {

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GameObject gridPlane = (GameObject)Instantiate(WhitePlane);
                GameObject gridPlaneBlack = (GameObject)Instantiate(BlackPlane);
                gridPlane.transform.SetParent(ParentGrid);
                gridPlaneBlack.transform.SetParent(ParentGrid);
                gridPlane.transform.position = new Vector3(gridPlane.transform.position.x - (x-width/2), 0.0f, gridPlane.transform.position.z - (z-height/2));
                gridPlaneBlack.transform.position = new Vector3(gridPlaneBlack.transform.position.x - (x - width / 2), 0.001f, gridPlaneBlack.transform.position.z -(z - height / 2));
                grid[x, z] = gridPlane;
                gridBlack[x, z] = gridPlaneBlack;
            }
        }
    }
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }
}
