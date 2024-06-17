using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class MazeScaler : MonoBehaviour
{
    public float xScale;
    public float yScale;
    public float zScale;



    public void Start()
    {
        transform.localScale = new Vector3(xScale, yScale, zScale);
    }

}