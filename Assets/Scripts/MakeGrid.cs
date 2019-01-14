using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeGrid : MonoBehaviour
{
    public GameObject WhitePlane;
    public GameObject BlackPlane;
    public int width = 10;
    public int height = 10;
    private GameObject[,] grid = new GameObject[10, 10];
    private GameObject[,] gridBlack = new GameObject[10, 10];

    private void Awake()
    {

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GameObject gridPlane = (GameObject)Instantiate(WhitePlane);
                GameObject gridPlaneBlack = (GameObject)Instantiate(BlackPlane);
                gridPlane.transform.position = new Vector3(gridPlane.transform.position.x + x, 0.0f, gridPlane.transform.position.z + z);
                gridPlaneBlack.transform.position = new Vector3(gridPlaneBlack.transform.position.x + x, 0.001f, gridPlaneBlack.transform.position.z + z);
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
