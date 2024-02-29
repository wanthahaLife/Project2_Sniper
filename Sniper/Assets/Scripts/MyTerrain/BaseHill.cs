using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHill : MonoBehaviour
{
    public int floorCount = 1;
    public Transform[] floors;
    public GameObject[] plains;
    public float[] floorWidth;
    public float[] floorLength;
    public GameObject[] stairs;
    public float[] stairsWidth;
    public float[] stairsLength;

    private void Awake()
    {
        floors = new Transform[floorCount];
        
    }
}
