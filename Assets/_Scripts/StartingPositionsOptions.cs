using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartingPositionsOptions : MonoBehaviour
{
    public static int TotalPerRow;
    public static float DistanceBetweenCars;
    public static float DistanceBetweenRows;

    public int totalPerRow = 3;
    public float distanceBetweenCars = 5f;
    public float distanceBetweenRows = 5f;

    private void Start()
    {
        TotalPerRow = totalPerRow;
        DistanceBetweenCars = distanceBetweenCars;
        DistanceBetweenRows = distanceBetweenRows;
    }
}
